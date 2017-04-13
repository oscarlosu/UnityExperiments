using UnityEditor;
using UnityEngine;
using PygmyMonkey.GifCreator.Utils;

namespace PygmyMonkey.GifCreator
{
	public class ParametersRenderer
	{
		private GifCreatorWindow gifCreatorWindow;

		public ParametersRenderer(GifCreatorWindow gifCreatorWindow)
		{
			this.gifCreatorWindow = gifCreatorWindow;

			updateCustomRectPreview();
		}

		public void OnPlayModeChange(bool isEditorPlaying)
		{
			updateCustomRectPreview();
		}

		public void DeactivateCustomRectPreview()
		{
			gifCreatorWindow.Parameters.showCustomRectPreview = false;
			updateCustomRectPreview();
		}

		private void updateCustomRectPreview()
		{
			if (CustomRectPreview.Singleton == null)
			{
				return;
			}

			if ((gifCreatorWindow.Parameters.section == RecorderParameters.Section.CustomRectAbsolute || gifCreatorWindow.Parameters.section == RecorderParameters.Section.CustomRectRelative) && gifCreatorWindow.Parameters.showCustomRectPreview)
			{
				CustomRectPreview.Singleton.Show(gifCreatorWindow.Parameters);
			}
			else
			{
				CustomRectPreview.Singleton.Hide();
			}
		}

		public void OnRecordingDone()
		{
			gifCreatorWindow.Parameters.outputWidth = gifCreatorWindow.Parameters.recordWidth;
		}

		public void drawInspector()
		{
			if (GUIUtils.DrawHeader("Parameters"))
			{
				using (new GUIUtils.GUIContents())
				{
					bool isUIEnabled = Recorder.Singleton != null && Recorder.Singleton.State == RecorderState.None;
					using (new GUIUtils.GUIEnabled(isUIEnabled))
					{
						using (new GUIUtils.GUIContents())
						{
							EditorGUI.BeginChangeCheck();
							gifCreatorWindow.Parameters.section = (RecorderParameters.Section)EditorGUILayout.EnumPopup("Screen section", gifCreatorWindow.Parameters.section);
							if (EditorGUI.EndChangeCheck())
							{
								updateCustomRectPreview();
							}

							EditorGUI.BeginChangeCheck();
							{
								switch (gifCreatorWindow.Parameters.section)
								{
								case RecorderParameters.Section.Fullscreen:
									using (new GUIUtils.GUIEnabled(false))
									{
										EditorGUILayout.IntField("Position X", (int)gifCreatorWindow.Parameters.getRectSection().x);
										EditorGUILayout.IntField("Position Y", (int)gifCreatorWindow.Parameters.getRectSection().y);
										EditorGUILayout.IntField("Width", (int)gifCreatorWindow.Parameters.getRectSection().width);
										EditorGUILayout.IntField("Height", (int)gifCreatorWindow.Parameters.getRectSection().height);
									}
									break;
									
								case RecorderParameters.Section.CustomRectAbsolute:
									gifCreatorWindow.Parameters.absolutePosX = EditorGUILayout.IntField("Position X", gifCreatorWindow.Parameters.absolutePosX);
									gifCreatorWindow.Parameters.absolutePosY = EditorGUILayout.IntField("Position Y", gifCreatorWindow.Parameters.absolutePosY);
									gifCreatorWindow.Parameters.absoluteWidth = EditorGUILayout.IntField("Width", gifCreatorWindow.Parameters.absoluteWidth);
									gifCreatorWindow.Parameters.absoluteHeight = EditorGUILayout.IntField("Height", gifCreatorWindow.Parameters.absoluteHeight);

									if (gifCreatorWindow.Parameters.absolutePosX < 0) gifCreatorWindow.Parameters.absolutePosX = 0;
									if (gifCreatorWindow.Parameters.absolutePosY < 0) gifCreatorWindow.Parameters.absolutePosY = 0;
									if (gifCreatorWindow.Parameters.absoluteWidth + gifCreatorWindow.Parameters.absolutePosX > gifCreatorWindow.Parameters.getMainGameViewSize().x) gifCreatorWindow.Parameters.absoluteWidth = (int)gifCreatorWindow.Parameters.getMainGameViewSize().x - gifCreatorWindow.Parameters.absolutePosX;
									if (gifCreatorWindow.Parameters.absoluteHeight + gifCreatorWindow.Parameters.absolutePosY > gifCreatorWindow.Parameters.getMainGameViewSize().y) gifCreatorWindow.Parameters.absoluteHeight = (int)gifCreatorWindow.Parameters.getMainGameViewSize().y - gifCreatorWindow.Parameters.absolutePosY;
									if (gifCreatorWindow.Parameters.absoluteWidth < 1) gifCreatorWindow.Parameters.absoluteWidth = 1;
									if (gifCreatorWindow.Parameters.absoluteHeight < 1) gifCreatorWindow.Parameters.absoluteHeight = 1;
									if (gifCreatorWindow.Parameters.absolutePosX >= gifCreatorWindow.Parameters.getMainGameViewSize().x) gifCreatorWindow.Parameters.absolutePosX = (int)gifCreatorWindow.Parameters.getMainGameViewSize().x - 1;
									if (gifCreatorWindow.Parameters.absolutePosY >= gifCreatorWindow.Parameters.getMainGameViewSize().y) gifCreatorWindow.Parameters.absolutePosY = (int)gifCreatorWindow.Parameters.getMainGameViewSize().y - 1;
									break;

								case RecorderParameters.Section.CustomRectRelative:
									gifCreatorWindow.Parameters.relativePosX = EditorGUILayout.Slider("Position X", gifCreatorWindow.Parameters.relativePosX, 0.0f, 0.99f);
									gifCreatorWindow.Parameters.relativePosY = EditorGUILayout.Slider("Position Y", gifCreatorWindow.Parameters.relativePosY, 0.0f, 0.99f);
									gifCreatorWindow.Parameters.relativeWidth = EditorGUILayout.Slider("Width", gifCreatorWindow.Parameters.relativeWidth, 0.01f, 1.0f);
									gifCreatorWindow.Parameters.relativeHeight = EditorGUILayout.Slider("Height", gifCreatorWindow.Parameters.relativeHeight, 0.01f, 1.0f);

									if (gifCreatorWindow.Parameters.relativePosX + gifCreatorWindow.Parameters.relativeWidth > 1.0f)
									{
										gifCreatorWindow.Parameters.relativeWidth = 1.0f - gifCreatorWindow.Parameters.relativePosX;
									}

									if (gifCreatorWindow.Parameters.relativePosY + gifCreatorWindow.Parameters.relativeHeight > 1.0f)
									{
										gifCreatorWindow.Parameters.relativeHeight = 1.0f - gifCreatorWindow.Parameters.relativePosY;
									}
									break;
								}

								switch (gifCreatorWindow.Parameters.section)
								{
								case RecorderParameters.Section.CustomRectAbsolute:
								case RecorderParameters.Section.CustomRectRelative:
									gifCreatorWindow.Parameters.customRectPreviewColor = EditorGUILayout.ColorField("Custom rect color", gifCreatorWindow.Parameters.customRectPreviewColor);
									gifCreatorWindow.Parameters.showCustomRectPreview = EditorGUILayout.Toggle("Preview on screen", gifCreatorWindow.Parameters.showCustomRectPreview);
									break;
								}
							}
							if (EditorGUI.EndChangeCheck())
							{
								updateCustomRectPreview();
							}
						}

						EditorGUI.BeginChangeCheck();
						{
							gifCreatorWindow.Parameters.framePerSecond = EditorGUILayout.IntSlider(new GUIContent("Frames Per Second", "The number of frames per second the gif will run at."), gifCreatorWindow.Parameters.framePerSecond, 1, 30);
							gifCreatorWindow.Parameters.duration = EditorGUILayout.Slider(new GUIContent("Duration", "The amount of time (in seconds) to record."), gifCreatorWindow.Parameters.duration, 0.1f, 30f);
						}
						if (EditorGUI.EndChangeCheck())
						{
							EditorPrefs.SetInt("gifcreator_fps", gifCreatorWindow.Parameters.framePerSecond);
							EditorPrefs.SetFloat("gifcreator_duration", gifCreatorWindow.Parameters.duration);
						}

						if (gifCreatorWindow.Parameters.repeat < -1) gifCreatorWindow.Parameters.repeat = -1;

						gifCreatorWindow.Parameters.recordWidth = EditorGUILayout.IntField(new GUIContent("Record width", "Even if your screen size is 1280x720 for example. You can specify a different width (640) that will be used to resize each recorded frame. Height is automatically calculated based on the recorded section ratio."), gifCreatorWindow.Parameters.recordWidth);
						EditorGUILayout.LabelField("Final record dimension", gifCreatorWindow.Parameters.getRecordWidth() + "x" + gifCreatorWindow.Parameters.getRecordHeight());

						EditorGUILayout.HelpBox("Estimated memory use: " + gifCreatorWindow.Parameters.getEstimatedMemoryUse().ToString("0") + "MB"
						+ "\n" + gifCreatorWindow.Parameters.getEstimatedMemoryUseDescription(), MessageType.Info);

						if (gifCreatorWindow.Parameters.recordMode == RecorderParameters.RecordMode.EVERYTHING)
						{
							if (gifCreatorWindow.Parameters.getRecordWidth() != gifCreatorWindow.Parameters.getRectSection().width)
							{
								EditorGUILayout.HelpBox("Be careful, your record width (" + gifCreatorWindow.Parameters.getRecordWidth() + ") is different than the screen width (" + gifCreatorWindow.Parameters.getRectSection().width + ") you want to record. Gif Creator will resize each recorded frame, each frame, this might result in bad performance and lags during the recording.", MessageType.Warning);
							}
						}
					}
				}
			}
		}
	}
}
