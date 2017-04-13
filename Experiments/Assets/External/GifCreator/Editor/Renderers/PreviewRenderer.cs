using UnityEditor;
using UnityEngine;
using PygmyMonkey.GifCreator.Utils;

namespace PygmyMonkey.GifCreator
{
	public class PreviewRenderer
	{
		private GifCreatorWindow gifCreatorWindow;

		public PreviewRenderer(GifCreatorWindow gifCreatorWindow)
		{
			this.gifCreatorWindow = gifCreatorWindow;
		}

		private Texture2D defaultTexture;

		private int frameIndex = 0;
		private float frameStart = 0;
		private float frameEnd = 0;

		public float FrameStart { get { return frameStart; } }
		public float FrameEnd { get { return frameEnd; } }

		private float lastFrameStart = 0;
		private float lastFrameEnd = 0;

		private float previewTime = 0.0f;
		private bool shouldPlayPreview = false;

		public void drawInspector()
		{
			if (defaultTexture == null)
			{
				defaultTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
				defaultTexture.hideFlags = HideFlags.HideAndDontSave;
				defaultTexture.SetPixel(0, 0, Color.black);
				defaultTexture.Apply();
			}

			if (GUIUtils.DrawHeader("Preview"))
			{
				using (new GUIUtils.GUIContents())
				{
					bool isEnabled = Recorder.Singleton != null;
					using (new GUIUtils.GUIEnabled(isEnabled && (Recorder.Singleton.State == RecorderState.Paused || Recorder.Singleton.State == RecorderState.None)))
					{
						ITexture[] frameArray = null;
						if (Recorder.Singleton != null && Recorder.Singleton.FrameList != null)
						{
							frameArray = Recorder.Singleton.FrameList.ToArray();
						}

						if (frameArray == null || frameArray.Length == 0)
						{
							frameIndex = 0;
							frameArray = new ITexture[0];
						}

						if (frameIndex >= frameArray.Length && frameArray.Length != 1)
						{
							frameIndex = frameArray.Length - 1;
						}

						if (frameIndex < 0)
						{
							frameIndex = 0;
						}

						float windowWidth = gifCreatorWindow.position.width - 45;
						float width = windowWidth;
						if (width > gifCreatorWindow.Parameters.getRecordWidth())
						{
							width = gifCreatorWindow.Parameters.getRecordWidth();
						}

						float height = width * gifCreatorWindow.Parameters.getRecordHeight() / (float)gifCreatorWindow.Parameters.getRecordWidth();
						
						Rect rect = EditorGUILayout.GetControlRect(false, height, GUILayout.Width(width));
						rect.x += 1;
						rect.x += (windowWidth - width) * 0.5f;

						// Draw the preview texture
						Texture currentRenderTexture = defaultTexture;

						if (frameArray != null && frameArray.Length >= 1)
						{
							currentRenderTexture = frameArray[frameIndex].GetTexture2D();
						}

						if (currentRenderTexture == null || (Recorder.Singleton != null && Recorder.Singleton.State == RecorderState.Recording))
						{
							currentRenderTexture = defaultTexture;
						}

						EditorGUI.DrawPreviewTexture(rect, currentRenderTexture);

						// Draw the controls
						using (new GUIUtils.GUIEnabled(GUI.enabled && frameArray.Length > 1))
						{
							float buttonWidth = 25;
							float combinedButtonWidth = buttonWidth * 4 + 5;
							float spaceNeeded = (windowWidth - combinedButtonWidth) * 0.5f;
							using (new GUIUtils.GUIHorizontal(GUILayout.Width(spaceNeeded)))
							{
								GUILayout.Space(spaceNeeded);

								GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fixedWidth = buttonWidth, fixedHeight = 25, margin = new RectOffset(5, 5, 5, 5), padding = new RectOffset(0, 0, -2, 0) };

								using (new GUIUtils.GUIEnabled(frameIndex < frameEnd))
								{
									if (GUILayout.Button(new GUIContent("\u21A6", "Set frameStart to current frame"), buttonStyle))
									{
										frameStart = frameIndex;
									}
								}

								if (GUILayout.Button(shouldPlayPreview ? new GUIContent("||", "Pause the preview") : new GUIContent(">", "Play the preview"), buttonStyle))
								{
									if (frameIndex < (int)frameStart || frameIndex > (int)frameEnd)
									{
										frameIndex = (int)frameStart;
									}

									previewTime = 0.0f;
									shouldPlayPreview = !shouldPlayPreview;
								}

								if (GUILayout.Button(new GUIContent("\u20DE", "Stop playing the preview"), buttonStyle))
								{
									frameIndex = 0;
									shouldPlayPreview = false;
								}

								using (new GUIUtils.GUIEnabled(GUI.enabled && frameIndex > frameStart))
								{
									if (GUILayout.Button(new GUIContent("\u21A4", "Set frameEnd to current frame"), buttonStyle))
									{
										frameEnd = frameIndex;
									}
								}
							}

							frameIndex = EditorGUILayout.IntSlider("Frame index", frameIndex, 0, frameArray.Length - 1);

							using (new GUIUtils.GUIHorizontal())
							{
								EditorGUILayout.MinMaxSlider(new GUIContent("Frames", "Here you can select wich frames to save"), ref frameStart, ref frameEnd, 0, frameArray.Length - 1);
								GUILayout.Space(55f);
							}
							
							if (lastFrameStart != frameStart && lastFrameEnd != FrameEnd)
							{
								frameIndex = (int)frameStart;
								lastFrameStart = frameStart;
								lastFrameEnd = frameEnd;
							}
							else if (lastFrameStart != frameStart)
							{
								frameIndex = (int)frameStart;
								lastFrameStart = frameStart;
							}
							else if (lastFrameEnd != frameEnd)
							{
								frameIndex = (int)frameEnd;
								lastFrameEnd = frameEnd;
							}
							
							frameStart = EditorGUILayout.IntField("Frame start", (int)frameStart);
							frameEnd = EditorGUILayout.IntField("Frame end", (int)frameEnd);

							if (frameStart < 0) frameStart = 0;
							if (frameEnd < 0) frameEnd = 0;
							if (frameStart > frameArray.Length - 1) frameStart = frameArray.Length - 1;
							if (frameEnd > frameArray.Length - 1) frameEnd = frameArray.Length - 1;
							if (frameStart > frameEnd) frameStart = frameEnd;

							int frameCount = (int)frameEnd - (int)frameStart + 1;
							EditorGUILayout.LabelField("Frame interval selected", (int)frameStart + " to " + (int)frameEnd + " (" + frameCount + " frames)");

							float duration = (frameCount / (float)gifCreatorWindow.Parameters.framePerSecond);
							string durationInfo = (frameCount > 1) ? (duration.ToString("0.00") + " seconds") : "-";
							EditorGUILayout.LabelField("Duration", durationInfo);
						}
					}
				}
			}
		}

		public void Pause()
		{
			shouldPlayPreview = false;
		}

		public void Update()
		{
			if (!shouldPlayPreview)
			{
				return;
			}

			if (Recorder.Singleton.FrameList == null || Recorder.Singleton.FrameList.Count == 0)
			{
				return;
			}

			previewTime += Time.unscaledDeltaTime;

			if (previewTime >= gifCreatorWindow.Parameters.getTimePerFrame())
			{
				if (frameIndex >= frameEnd)
				{
					frameIndex = (int)frameStart;
				}
				else
				{
					frameIndex++;
				}

				gifCreatorWindow.Repaint();

				previewTime -= gifCreatorWindow.Parameters.getTimePerFrame();
			}
		}

		public void OnRecordingDone()
		{
			frameIndex = 0;
			lastFrameStart = frameStart = 0;
			lastFrameEnd = frameEnd = Recorder.Singleton.FrameList.Count - 1;

			shouldPlayPreview = false;
		}
	}
}
