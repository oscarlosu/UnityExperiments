// This script is based on the work of Thomas Hourdel, but has been heavily modified
// https://github.com/Chman/Moments

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Moments;
using Moments.Encoder;
using ThreadPriority = System.Threading.ThreadPriority;

namespace PygmyMonkey.GifCreator
{
	public enum RecorderState
	{
		None,
		Recording,
		Paused,
		PreProcessing,
		Saving,
	}

	public class NTexture : ITexture
	{
		private Texture2D texture2D;

		public NTexture(Texture2D texture2D)
		{
			this.texture2D = texture2D;
		}

		public void Flush()
		{
			if (texture2D != null)
			{
				Texture2D.DestroyImmediate(texture2D);
			}
		}

		public void Convert(RecorderParameters parameters)
		{
		}

		public Texture2D GetTexture2D()
		{
			return texture2D;
		}
	}

	public class RTexture : ITexture
	{
		private RenderTexture renderTexture;
		private Texture2D texture2D;

		public RTexture(RenderTexture renderTexture)
		{
			this.renderTexture = renderTexture;
		}

		public void Flush()
		{
			if (renderTexture != null)
			{
				Texture2D.DestroyImmediate(renderTexture);
			}

			if (texture2D != null)
			{
				Texture2D.DestroyImmediate(texture2D);
			}
		}

		public void Convert(RecorderParameters parameters)
		{
			texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
			texture2D.hideFlags = HideFlags.HideAndDontSave;
			texture2D.wrapMode = TextureWrapMode.Clamp;
			texture2D.filterMode = FilterMode.Bilinear;
			texture2D.anisoLevel = 0;

			RenderTexture.active = renderTexture;
			texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
			texture2D.Apply();
			RenderTexture.active = null;

			Texture2D.DestroyImmediate(renderTexture);
		}

		public Texture2D GetTexture2D()
		{
			return texture2D;
		}
	}

	public interface ITexture
	{
		void Flush();
		void Convert(RecorderParameters parameters);
		Texture2D GetTexture2D();
	}

	[ExecuteInEditMode]
	public sealed class Recorder : MonoBehaviour
	{
		public static Recorder Singleton;

		private RecorderParameters parameters;

		#region Public fields

		/// <summary>
		/// Current state of the recorder.
		/// </summary>
		public RecorderState State { get; private set; }
		public List<ITexture> FrameList { get; private set; }

		#endregion

		#region Delegates

		/// <summary>
		/// Called every time a frame is recorded
		/// </summary>
		public Action<float> OnRecordingProgress;

		/// <summary>
		/// Called when the recording is done.
		/// </summary>
		public Action OnRecordingDone;

		/// <summary>
		/// Called when the pre-processing step has finished.
		/// </summary>
		public Action OnPreProcessingDone;

		/// <summary>
		/// Called by each worker thread every time a frame is processed during the save process.
		/// The first parameter holds the worker ID and the second one a value in range [0;1] for
		/// the actual progress. This callback is probably not thread-safe, use at your own risks.
		/// </summary>
		public Action<int, float> OnFileSaveProgress;

		/// <summary>
		/// Called once a gif file has been saved. The first parameter will hold the worker ID and
		/// the second one the absolute file path.
		/// </summary>
		public Action<int, string> OnFileSaved;

		#endregion

		#region Internal fields

		Material customBlitMaterial;
		int m_MaxFrameCount;
		float m_Time;

		#endregion

		#region Public API

		public void Reset()
		{
			State = RecorderState.None;

			// Start fresh
			FlushMemory();
		}

		public void Init(RecorderParameters parameters)
		{
			this.parameters = parameters;

			Reset();

			// Init
			m_MaxFrameCount = Mathf.RoundToInt(parameters.duration * parameters.framePerSecond);
			m_Time = 0f;
		}

		/// <summary>
		/// Pauses recording.
		/// </summary>
		public void Pause()
		{
			if (State == RecorderState.PreProcessing)
			{
				Debug.LogWarning("Attempting to pause recording during the pre-processing step. The recorder is automatically paused when pre-processing.");
				return;
			}

			State = RecorderState.Paused;
		}

		/// <summary>
		/// Starts or resumes recording. You can't resume while it's pre-processing data to be saved.
		/// </summary>
		public void Record()
		{
			if (State == RecorderState.PreProcessing)
			{
				Debug.LogWarning("Attempting to resume recording during the pre-processing step.");
				return;
			}

			State = RecorderState.Recording;
		}

		/// <summary>
		/// Clears all saved frames from memory and starts fresh.
		/// </summary>
		public void FlushMemory()
		{
			if (State == RecorderState.PreProcessing)
			{
				Debug.LogWarning("Attempting to flush memory during the pre-processing step.");
				return;
			}

			if (FrameList != null)
			{
				foreach (ITexture t in FrameList)
					t.Flush();
				
				FrameList.Clear();
			}
		}

		/// <summary>
		/// Saves the stored frames to a gif file. The filename will automatically be generated.
		/// Recording will be paused and won't resume automatically. You can use the 
		/// <code>OnPreProcessingDone</code> callback to be notified when the pre-processing
		/// step has finished.
		/// </summary>
		public void Save(int frameStart, int frameEnd)
		{
			Save(GenerateFileName(), frameStart, frameEnd);
		}

		/// <summary>
		/// Saves the stored frames to a gif file. If the filename is null or empty, an unique one
		/// will be generated. You don't need to add the .gif extension to the name. Recording will
		/// be paused and won't resume automatically. You can use the <code>OnPreProcessingDone</code>
		/// callback to be notified when the pre-processing step has finished.
		/// </summary>
		/// <param name="filename">File name without extension</param>
		public void Save(string filename, int frameStart, int frameEnd)
		{
			if (State == RecorderState.PreProcessing)
			{
				Debug.LogWarning("Attempting to save during the pre-processing step.");
				return;
			}

			if (FrameList.Count == 0)
			{
				Debug.LogWarning("Nothing to save. Maybe you forgot to start the recorder ?");
				return;
			}

			State = RecorderState.PreProcessing;

			if (string.IsNullOrEmpty(filename))
				filename = GenerateFileName();

			StartCoroutine(PreProcess(filename, frameStart, frameEnd));
		}

		public void StopRecording()
		{
			State = RecorderState.Paused;

			foreach (ITexture t in FrameList)
			{
				t.Convert(parameters);
			}

			if (OnRecordingDone != null)
			{
				OnRecordingDone();
			}
		}

		#endregion

		#region Unity events

		void Awake()
		{
			FrameList = new List<ITexture>();
			customBlitMaterial = new Material(Shader.Find("Hidden/BlitCopy"));
			customBlitMaterial.hideFlags = HideFlags.HideAndDontSave;

			State = RecorderState.None;
		}

		public void OnEnable()
		{
			Singleton = this;
		}

		void OnDestroy()
		{
			FlushMemory();
		}

		void OnGUI()
		{
			if (parameters == null || !parameters.recordMode.Equals(RecorderParameters.RecordMode.EVERYTHING))
			{
				return;
			}

			if (State != RecorderState.Recording)
			{
				return;
			}

			if (Event.current.type != EventType.Repaint)
			{
 				return;
 			}

 			recordFrame(() =>
 			{
				RenderTexture.active = null;

				Texture2D texture = new Texture2D((int)parameters.getRectSection().width, (int)parameters.getRectSection().height, TextureFormat.ARGB32, false);
				texture.filterMode = FilterMode.Bilinear;
				texture.ReadPixels(parameters.getRectSection(), 0, 0, false);
				texture.Apply();

				resize(texture, parameters.getRecordWidth(), parameters.getRecordHeight());

				FrameList.Add(new NTexture(texture));
 			});
		}

		void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (parameters == null || !parameters.recordMode.Equals(RecorderParameters.RecordMode.SINGLE_CAMERA))
			{
				Graphics.Blit(source, destination);
				return;
			}

			if (State != RecorderState.Recording)
			{
				Graphics.Blit(source, destination);
				return;
			}

 			recordFrame(() =>
 			{
				RenderTexture rt = new RenderTexture(parameters.getRecordWidth(), parameters.getRecordHeight(), 0, RenderTextureFormat.ARGB32);
				rt.wrapMode = TextureWrapMode.Clamp;
				rt.filterMode = FilterMode.Bilinear;
				rt.anisoLevel = 0;

				CustomGraphicsBlit(source, rt);

				FrameList.Add(new RTexture(rt));
			});

			Graphics.Blit(source, destination);
 		}

		void CustomGraphicsBlit(RenderTexture source, RenderTexture dest)
        {
            RenderTexture.active = dest;

			customBlitMaterial.SetTexture("_MainTex", source);

            GL.PushMatrix();
            GL.LoadOrtho();

			customBlitMaterial.SetPass(0);

            GL.Begin(GL.QUADS);

			Rect rect = parameters.getRectSection();
			float bottom = rect.y / source.height;
			float top = (rect.y + rect.height) / source.height;
			float left = rect.x / source.width;
			float right = (rect.x + rect.width) / source.width;

			float bottomPosition = 1.0f;
			float topPosition = 0.0f;

			if (parameters.recordUpsideDown)
			{
				bottomPosition = 0.0f;
				topPosition = 1.0f;
			}

			GL.MultiTexCoord2(0, left, bottom);
			GL.Vertex3(0.0f, bottomPosition, 0.0f); // BL

			GL.MultiTexCoord2(0, right, bottom);
			GL.Vertex3(1.0f, bottomPosition, 0.0f); // BR

            GL.MultiTexCoord2(0, right, top);
			GL.Vertex3(1.0f, topPosition, 0.0f); // TR

			GL.MultiTexCoord2(0, left, top);
			GL.Vertex3(0.0f, topPosition, 0.0f); // TL

            GL.End();
            GL.PopMatrix();
        }

 		private void recordFrame(Action onRecord)
 		{
			m_Time += Time.unscaledDeltaTime;

			if (m_Time >= parameters.getTimePerFrame())
			{
				switch (parameters.recordType)
				{
					case RecorderParameters.RecordType.Duration:
						if (FrameList.Count >= m_MaxFrameCount)
						{
							StopRecording();
							return;
						}
						break;

					case RecorderParameters.RecordType.LastSeconds:
						// Limit the amount of frames stored in memory
						if (FrameList.Count >= m_MaxFrameCount)
						{
							FrameList[0].Flush();
							FrameList.RemoveAt(0);
						}
						break;
				}

				m_Time -= parameters.getTimePerFrame();

				onRecord();

				if (parameters.recordType == RecorderParameters.RecordType.Duration)
				{
					if (OnRecordingProgress != null)
					{
						OnRecordingProgress(FrameList.Count / (float)m_MaxFrameCount);
					}
				}
			}
		}

		#endregion

		#region Methods

		// Flushes a single Texture object
		void Flush(Texture texture)
		{
			Texture2D.DestroyImmediate(texture);
		}

		// Gets a filename : GifCapture-yyyyMMddHHmmssffff
		string GenerateFileName()
		{
			string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
			return "GifCapture-" + timestamp;
		}

		// Pre-processing coroutine to extract frame data and send everything to a separate worker thread
		IEnumerator PreProcess(string filename, int frameStart, int frameEnd)
		{
			List<GifFrame> frames = new List<GifFrame>(frameEnd - frameStart);

			// Process the frame queue
			for (int i = frameStart; i <= frameEnd; i++)
			{
				Texture2D texture = new Texture2D(FrameList[i].GetTexture2D().width, FrameList[i].GetTexture2D().height, FrameList[i].GetTexture2D().format, false);
				texture.SetPixels32(FrameList[i].GetTexture2D().GetPixels32());
				texture.Apply();

				resize(texture, parameters.getOutputWidth(), parameters.getOutputHeight());

				GifFrame frame = new GifFrame() { Width = texture.width, Height = texture.height, Data = texture.GetPixels32() };
				frames.Add(frame);

				yield return null;
			}

			// Callback
			if (OnPreProcessingDone != null)
			{
				OnPreProcessingDone();
			}

			State = RecorderState.Saving;

			if (!Directory.Exists(parameters.getDestinationFolder()))
			{
				Directory.CreateDirectory(parameters.getDestinationFolder());
			}

			// Setup a worker thread and let it do its magic
			GifEncoder encoder = new GifEncoder(parameters.repeat, parameters.quality);
			encoder.SetDelay(Mathf.RoundToInt(parameters.getTimePerFrame() * 1000f));
			Worker worker = new Worker(parameters.workerPriority)
			{
				m_Encoder = encoder,
				m_Frames = frames,
				m_FilePath = parameters.getDestinationFolder() + "/" + filename + ".gif",
				m_OnFileSaved = OnFileSaved,
				m_OnFileSaveProgress = OnFileSaveProgress
			};
			worker.Start();
		}

		private static void resize(Texture2D texture, int width, int height)
		{
			if (texture.width == width && texture.height == height)
			{
				return;
			}

			Color[] pixelArray = new Color[width * height];
			float incX = (1.0f / (float)width);
			float incY = (1.0f / (float)height); 

			for (int px = 0; px < pixelArray.Length; px++)
			{ 
				pixelArray[px] = texture.GetPixelBilinear(incX * ((float)px % width), incY * ((float)Mathf.Floor(px / width))); 
			}

			texture.Resize(width, height);
			texture.SetPixels(pixelArray, 0); 
			texture.Apply();
		}

		#endregion
	}
}
