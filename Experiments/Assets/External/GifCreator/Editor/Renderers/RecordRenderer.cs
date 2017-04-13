using UnityEditor;
using UnityEngine;
using PygmyMonkey.GifCreator.Utils;
using System.Collections.Generic;

namespace PygmyMonkey.GifCreator
{
	public class RecordRenderer
	{
		private GifCreatorWindow gifCreatorWindow;

		public RecordRenderer(GifCreatorWindow gifCreatorWindow)
		{
			this.gifCreatorWindow = gifCreatorWindow;
		}

		private List<string> warningList = new List<string>();
		private List<string> errorList = new List<string>();

		public void drawInspector()
		{
			if (GUIUtils.DrawHeader("Record"))
			{
				using (new GUIUtils.GUIContents())
				{
					bool isButtonEnabled = errorList.Count == 0 && Application.isPlaying;
					if (Recorder.Singleton != null)
					{
						isButtonEnabled &= Recorder.Singleton.State == RecorderState.None;
					}

					EditorGUI.BeginChangeCheck();
					{
						//gifCreatorWindow.Parameters.pauseAfterRecord = EditorGUILayout.Toggle(new GUIContent("Pause after recording", "Pause the Unity Editor after recording is done."), gifCreatorWindow.Parameters.pauseAfterRecord);

						if (gifCreatorWindow.Parameters.recordMode == RecorderParameters.RecordMode.SINGLE_CAMERA)
						{
							gifCreatorWindow.Parameters.recordUpsideDown = EditorGUILayout.Toggle(new GUIContent("Record Upside Down", "If, when recording, the image is upside down, check this box!"), gifCreatorWindow.Parameters.recordUpsideDown);
						}
					}
					if (EditorGUI.EndChangeCheck())
					{
						EditorPrefs.SetBool("gifcreator_pause_after_record", gifCreatorWindow.Parameters.pauseAfterRecord);
						EditorPrefs.SetBool("gifcreator_record_upsidedown", gifCreatorWindow.Parameters.recordUpsideDown);
					}

					gifCreatorWindow.Parameters.recordType = (RecorderParameters.RecordType)EditorGUILayout.EnumPopup("Record type", gifCreatorWindow.Parameters.recordType);

					switch (gifCreatorWindow.Parameters.recordType)
					{
						case RecorderParameters.RecordType.Duration:
						EditorGUILayout.HelpBox("Pressing record will record for " + gifCreatorWindow.Parameters.duration + " seconds. You can also stop recording whenever you want, by pressing the stop recording button.", MessageType.Info);
						break;

						case RecorderParameters.RecordType.LastSeconds:
						EditorGUILayout.HelpBox("Pressing record will start recording. Once you press the stop recording button, you will have the last " + gifCreatorWindow.Parameters.duration + " seconds.", MessageType.Info);
						break;
					}


					float buttonWidth = 100;
					float combinedButtonWidth = buttonWidth * 3 + 5;
					float windowWidth = gifCreatorWindow.position.width - 45;
					float spaceNeeded = (windowWidth - combinedButtonWidth) * 0.5f;
					using (new GUIUtils.GUIHorizontal(GUILayout.Width(spaceNeeded)))
					{
						GUILayout.Space(spaceNeeded);

						GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fixedWidth = buttonWidth, fixedHeight = 25, margin = new RectOffset(5, 5, 5, 5), padding = new RectOffset(0, 0, -2, 0) };

						using (new GUIUtils.GUIEnabled(isButtonEnabled))
						{
							if (GUILayout.Button("Record", buttonStyle))
							{
								GUI.FocusControl(null);

								gifCreatorWindow.ParametersRenderer.DeactivateCustomRectPreview();

								switch (gifCreatorWindow.Parameters.recordType)
								{
									case RecorderParameters.RecordType.Duration:
									gifCreatorWindow.ProgressRenderer.UpdateProgress("Recording", 0.0f);
									break;

									case RecorderParameters.RecordType.LastSeconds:
									gifCreatorWindow.ProgressRenderer.UpdateProgress("Recording. Keeping the last " + gifCreatorWindow.Parameters.duration + " seconds", 0.0f);
									break;
								}

								Recorder.Singleton.Init(gifCreatorWindow.Parameters);
								Recorder.Singleton.Record();
							}
						}

						using (new GUIUtils.GUIEnabled(Recorder.Singleton != null && Recorder.Singleton.State == RecorderState.Recording))
						{
							if (GUILayout.Button("Stop Recording", buttonStyle))
							{
								GUI.FocusControl(null);

								Recorder.Singleton.StopRecording();
							}
						}

						if (GUILayout.Button("Reset", buttonStyle))
						{
							if (Recorder.Singleton.State == RecorderState.Paused)
							{
								if (EditorUtility.DisplayDialog("Erase the current gif?", "Are you sure you want to reset the current gif, so you can record a new one?", "Erase", "Cancel"))
								{
									gifCreatorWindow.Reset();
								}
							}
							else
							{
								gifCreatorWindow.Reset();
							}
						}
					}

					drawWarningsAndErrors();
				}
			}
		}

		private void drawWarningsAndErrors()
		{
			warningList.Clear();
			errorList.Clear();

			if (!Application.isPlaying)
			{
				warningList.Add("You must be in play mode to hit the record button");
			}

			for (int i = 0; i < warningList.Count; i++)
			{
				EditorGUILayout.HelpBox(warningList[i], MessageType.Warning);
			}

			for (int i = 0; i < errorList.Count; i++)
			{
				EditorGUILayout.HelpBox(errorList[i], MessageType.Error);
			}
		}

		public void OnRecordingProgress(float percent)
		{
			gifCreatorWindow.ProgressRenderer.UpdateProgress("Recording", percent);
			gifCreatorWindow.Repaint();
		}

		public void OnRecordingDone()
		{
			if (gifCreatorWindow.Parameters.pauseAfterRecord)
			{
				EditorApplication.isPaused = true;
			}

			gifCreatorWindow.ProgressRenderer.UpdateProgress("Recording Done", 1.0f);
			gifCreatorWindow.Repaint();
		}

		public void OnPreProcessingDone()
		{
			// All frames have been extracted and sent to a worker thread for compression !
			// The Recorder is ready to record again, you can call Record() here if you don't
			// want to wait for the file to be compresse and saved.
			// Pre-processing is done in the main thread, but frame compression & file saving
			// has its own thread, so you can save multiple gif at once.

			gifCreatorWindow.ProgressRenderer.UpdateProgress("PreProcessing Done, Start saving", 0.0f);
			gifCreatorWindow.Repaint();
		}

		public void OnFileSaveProgress(int id, float percent)
		{
			// This callback is probably not thread safe so use it at your own risks.
			// Percent is in range [0;1] (0 being 0%, 1 being 100%).

			gifCreatorWindow.ProgressRenderer.UpdateProgress("Saving", percent);
			repaintWindow = true;
		}

		public void OnFileSaved(int id, string filepath)
		{
			gifCreatorWindow.ProgressRenderer.UpdateProgress("Saving done!", 1.0f);
			repaintWindow = true;

			Recorder.Singleton.Pause();
		}

		private bool repaintWindow = false;
		public void Update()
		{
			if (repaintWindow)
			{
				gifCreatorWindow.Repaint();
				repaintWindow = false;
			}
		}
	}
}
