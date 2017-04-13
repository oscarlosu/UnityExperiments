using UnityEditor;
using UnityEngine;
using PygmyMonkey.GifCreator.Utils;

namespace PygmyMonkey.GifCreator
{
	public class EmptyRenderer
	{
		private GifCreatorWindow gifCreatorWindow;

		public EmptyRenderer(GifCreatorWindow gifCreatorWindow)
		{
			this.gifCreatorWindow = gifCreatorWindow;
		}

		private void assignCallbacks()
		{
			if (Recorder.Singleton == null)
			{
				return;
			}

			Recorder.Singleton.OnRecordingProgress += delegate(float percent)
			{
				gifCreatorWindow.RecordRenderer.OnRecordingProgress(percent);
			};

			Recorder.Singleton.OnRecordingDone += delegate()
			{
				gifCreatorWindow.RecordRenderer.OnRecordingDone();
				gifCreatorWindow.PreviewRenderer.OnRecordingDone();
				gifCreatorWindow.ParametersRenderer.OnRecordingDone();
			};

			Recorder.Singleton.OnPreProcessingDone += delegate()
			{
				gifCreatorWindow.RecordRenderer.OnPreProcessingDone();
			};

			Recorder.Singleton.OnFileSaveProgress += delegate(int threadId, float percent)
			{
				gifCreatorWindow.RecordRenderer.OnFileSaveProgress(threadId, percent);
			};

			Recorder.Singleton.OnFileSaved += delegate(int threadId, string filePath)
			{
				gifCreatorWindow.RecordRenderer.OnFileSaved(threadId, filePath);
				gifCreatorWindow.ExportRenderer.OnFileSaved(threadId, filePath);
			};
		}

		public void Update()
		{
			if (CustomRectPreview.Singleton == null)
			{
				createCustomRectPreview();
			}

			switch (gifCreatorWindow.Parameters.recordMode)
			{
			case RecorderParameters.RecordMode.SINGLE_CAMERA:
				if (gifCreatorWindow.Parameters.camera == null)
				{
					destroyRecorder();
				}

				if (Recorder.Singleton == null && gifCreatorWindow.Parameters.camera != null)
				{
					createRecorder();
				}
				break;

			case RecorderParameters.RecordMode.EVERYTHING:
				if (Recorder.Singleton == null)
				{
					createRecorder();
				}
				break;
			}
		}

		public void OnPlayModeChange(bool isEditorPlaying)
		{
			gifCreatorWindow.Repaint();

			if (isEditorPlaying)
			{
				assignCallbacks();
			}
			else
			{
				gifCreatorWindow.Reset();
			}
		}

		private Camera lastCamera;
		public void drawInspector()
		{
			if (GUIUtils.DrawHeader("Camera"))
			{
				using (new GUIUtils.GUIContents())
				{
					bool isUIEnabled = Recorder.Singleton == null || Recorder.Singleton.State == RecorderState.None;
					using (new GUIUtils.GUIEnabled(isUIEnabled))
					{
						EditorGUI.BeginChangeCheck();
						gifCreatorWindow.Parameters.recordMode = (RecorderParameters.RecordMode)GUILayout.Toolbar((int)gifCreatorWindow.Parameters.recordMode, new string[] {"Record single camera (fast)", "Record everything (slow)"});
						if (EditorGUI.EndChangeCheck())
						{
							destroyRecorder();
							createRecorder();
							assignCallbacks();
						}

						switch (gifCreatorWindow.Parameters.recordMode)
						{
							case RecorderParameters.RecordMode.SINGLE_CAMERA:
							if (gifCreatorWindow.Parameters.camera == null)
							{
								EditorGUILayout.HelpBox("You first need to specify a camera that will be use to record things on screen", MessageType.Warning);
							}

							EditorGUI.BeginChangeCheck();
							gifCreatorWindow.Parameters.camera = (Camera)EditorGUILayout.ObjectField("Camera", gifCreatorWindow.Parameters.camera, typeof(Camera), true);
							if (EditorGUI.EndChangeCheck())
							{
								if (gifCreatorWindow.Parameters.camera == null)
								{
									destroyRecorder();
								}
								else
								{
									if (lastCamera == null)
									{
										destroyRecorder();
										createRecorder();
										assignCallbacks();
									}

									lastCamera = gifCreatorWindow.Parameters.camera;
								}
							}
							break;

							case RecorderParameters.RecordMode.EVERYTHING:
							EditorGUILayout.HelpBox("You decided to record the entire Game view. Note that this is a really slow method, that will impact FPS if used on a large complex scene. But it will record everything in the Game view (multiple cameras, UI etc...) and not just one camera.", MessageType.Warning);
							break;
						}
					}
				}
			}
		}

		private void destroyRecorder()
		{
			if (gifCreatorWindow.Parameters.camera != null)
			{
				Component.DestroyImmediate(gifCreatorWindow.Parameters.camera.GetComponent<Recorder>());
				Component.DestroyImmediate(gifCreatorWindow.Parameters.camera.GetComponent<CustomRectPreview>());
			}

			if (lastCamera != null)
			{
				Component.DestroyImmediate(lastCamera.GetComponent<Recorder>());
				Component.DestroyImmediate(lastCamera.GetComponent<CustomRectPreview>());
			}

			if (Recorder.Singleton != null)
			{
				Component.DestroyImmediate(Recorder.Singleton);
			}
		}

		public void OnDestroy()
		{
			destroyRecorder();

			if (CustomRectPreview.Singleton != null)
			{
				Component.DestroyImmediate(CustomRectPreview.Singleton);
			}
		}

		private void createRecorder()
		{
			GameObject recordParent = null;

			if (gifCreatorWindow.Parameters.recordMode == RecorderParameters.RecordMode.SINGLE_CAMERA)
			{
				if (gifCreatorWindow.Parameters.camera == null)
				{
					return;
				}

				recordParent = gifCreatorWindow.Parameters.camera.gameObject;
			}
			else if (gifCreatorWindow.Parameters.recordMode == RecorderParameters.RecordMode.EVERYTHING)
			{
				recordParent = CustomRectPreview.Singleton.gameObject;
			}

			Recorder recorder = recordParent.GetComponent<Recorder>();
			if (recorder == null)
			{
				recorder = recordParent.AddComponent<Recorder>();
				recorder.hideFlags = HideFlags.DontSaveInBuild | HideFlags.HideInInspector;
				//recorder.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
			}

			recorder.Init(gifCreatorWindow.Parameters);
			Recorder.Singleton = recorder;
		}

		private void createCustomRectPreview()
		{
			GameObject customRectPreviewGameObject = GameObject.Find("[Gif Creator] Custom Rect Preview");
			if (customRectPreviewGameObject == null)
			{
				customRectPreviewGameObject = new GameObject("[Gif Creator] Custom Rect Preview");
				customRectPreviewGameObject.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
			}

			CustomRectPreview customRectPreview = customRectPreviewGameObject.GetComponent<CustomRectPreview>();
			if (customRectPreview == null)
			{
				customRectPreview = customRectPreviewGameObject.AddComponent<CustomRectPreview>();
			}

			CustomRectPreview.Singleton = customRectPreview;
		}
	}
}
