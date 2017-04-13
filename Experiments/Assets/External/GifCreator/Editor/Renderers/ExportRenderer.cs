using UnityEditor;
using UnityEngine;
using PygmyMonkey.GifCreator.Utils;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using ThreadPriority = System.Threading.ThreadPriority;

namespace PygmyMonkey.GifCreator
{
	public class ExportRenderer
	{
		private GifCreatorWindow gifCreatorWindow;
		private string lastDestinationPath;
		private string lastSavedFilePath;
		private bool shouldExportAfterSave = false;

		public ExportRenderer(GifCreatorWindow gifCreatorWindow)
		{
			this.gifCreatorWindow = gifCreatorWindow;
		}

		public void drawInspector()
		{
			if (GUIUtils.DrawHeader("Export"))
			{
				using (new GUIUtils.GUIContents())
				{
					bool isButtonEnabled = Recorder.Singleton != null && Recorder.Singleton.State == RecorderState.Paused;
					isButtonEnabled &= ((int)gifCreatorWindow.PreviewRenderer.FrameEnd - (int)gifCreatorWindow.PreviewRenderer.FrameStart > 0);
					using (new GUIUtils.GUIEnabled(isButtonEnabled))
					{
						EditorGUI.BeginChangeCheck();
						{
							gifCreatorWindow.Parameters.repeat = EditorGUILayout.IntField(new GUIContent("Repeat", "-1 to disable, 0 to loop forever, x to loop x times."), gifCreatorWindow.Parameters.repeat);
							gifCreatorWindow.Parameters.quality = EditorGUILayout.IntSlider(new GUIContent("Compression Quality", "Lower values mean better quality but slightly longer processing time. 15 is generally a good middleground value."), gifCreatorWindow.Parameters.quality, 1, 100);
						}
						if (EditorGUI.EndChangeCheck())
						{
							EditorPrefs.SetInt("gifcreator_repeat", gifCreatorWindow.Parameters.repeat);
							EditorPrefs.SetInt("gifcreator_quality", gifCreatorWindow.Parameters.quality);
						}
						gifCreatorWindow.Parameters.outputWidth = EditorGUILayout.IntField(new GUIContent("Output width", "The final width that will be used to export the gif"), gifCreatorWindow.Parameters.outputWidth);
						EditorGUILayout.LabelField("Final output dimension", gifCreatorWindow.Parameters.getOutputWidth() + "x" + gifCreatorWindow.Parameters.getOutputHeight());

						gifCreatorWindow.Parameters.workerPriority = (ThreadPriority)EditorGUILayout.EnumPopup(new GUIContent("Thread priority", "Thread priority to use when processing frames to a gif file."), gifCreatorWindow.Parameters.workerPriority);

						using (new GUIUtils.GUIHorizontal())
						{
							gifCreatorWindow.Parameters.destinationFolder = EditorGUILayout.TextField("Destination folder", gifCreatorWindow.Parameters.destinationFolder);
							
							if (GUILayout.Button("Select folder", EditorStyles.miniButton, GUILayout.Width(70f)))
							{
								GUI.FocusControl(null);

								gifCreatorWindow.Parameters.destinationFolder = EditorUtility.OpenFolderPanel("Gif destination folder", gifCreatorWindow.Parameters.getDestinationFolder(), null);
								EditorPrefs.SetString("gifcreator_destination_folder", gifCreatorWindow.Parameters.destinationFolder);
							}
						}

						EditorGUI.BeginChangeCheck();
						{
							gifCreatorWindow.Parameters.openFolderAfterSave = EditorGUILayout.Toggle(new GUIContent("Open folder after save", "Open the folder where your gif has been saved."), gifCreatorWindow.Parameters.openFolderAfterSave);
						}
						if (EditorGUI.EndChangeCheck())
						{
							EditorPrefs.SetBool("gifcreator_open_after_save", gifCreatorWindow.Parameters.openFolderAfterSave);
						}

						drawWarningsAndErrors();

						float buttonWidth = 125;
						float combinedButtonWidth = buttonWidth * 2 + 5;
						float windowWidth = gifCreatorWindow.position.width - 45;
						float spaceNeeded = (windowWidth - combinedButtonWidth) * 0.5f;
						using (new GUIUtils.GUIHorizontal(GUILayout.Width(spaceNeeded)))
						{
							GUILayout.Space(spaceNeeded);

							GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fixedWidth = buttonWidth, fixedHeight = 25, margin = new RectOffset(5, 5, 5, 5), padding = new RectOffset(0, 0, -2, 0) };

							if (GUILayout.Button("Save", buttonStyle))
							{
								shouldExportAfterSave = false;

								lastDestinationPath = gifCreatorWindow.Parameters.destinationFolder;

								gifCreatorWindow.PreviewRenderer.Pause();
								gifCreatorWindow.ProgressRenderer.UpdateProgress("Preprocessing (this can take a few minutes...)", 0.0f);
								Recorder.Singleton.Save((int)gifCreatorWindow.PreviewRenderer.FrameStart, (int)gifCreatorWindow.PreviewRenderer.FrameEnd);
							}

							if (GUILayout.Button("Upload to imgur.com", buttonStyle))
							{
								gifCreatorWindow.PreviewRenderer.Pause();
								
								if (File.Exists(lastSavedFilePath))
								{
									upload();
								}
								else
								{
									shouldExportAfterSave = true;

									lastDestinationPath = gifCreatorWindow.Parameters.destinationFolder;
									gifCreatorWindow.Parameters.destinationFolder = Application.persistentDataPath;

									gifCreatorWindow.ProgressRenderer.UpdateProgress("Preprocessing (this can take a few minutes...)", 0.0f);
									Recorder.Singleton.Save((int)gifCreatorWindow.PreviewRenderer.FrameStart, (int)gifCreatorWindow.PreviewRenderer.FrameEnd);
								}
							}
						}
					}
				}
			}
		}

		private bool triggerSaveCallbackOnMainThread = false;
		public void OnFileSaved(int threadId, string filePath)
		{
			lastSavedFilePath = filePath;
			gifCreatorWindow.Parameters.destinationFolder = lastDestinationPath;

			if (shouldExportAfterSave)
			{
				upload();
			}

			triggerSaveCallbackOnMainThread = !shouldExportAfterSave;
			shouldExportAfterSave = false;
		}

		public void Update()
		{
			if (triggerSaveCallbackOnMainThread)
			{
				Debug.Log("Gif file has been saved to: " + lastSavedFilePath);
				if (gifCreatorWindow.Parameters.openFolderAfterSave)
				{
					EditorUtility.RevealInFinder(lastSavedFilePath);
				}

				triggerSaveCallbackOnMainThread = false;
			}
		}

		public void Reset()
		{
			lastSavedFilePath = null;
		}

		private long getLastSaveFileSize()
		{
			if (string.IsNullOrEmpty(lastSavedFilePath) || !File.Exists(lastSavedFilePath))
			{
				return -1;
			}

			FileInfo fileInfo = new FileInfo(lastSavedFilePath);
			return fileInfo.Length;
		}

		private string getLastSaveFileSizeDescription()
		{
			float fileSize = getLastSaveFileSize();
			if (fileSize > 1000)
			{
				fileSize /= 1024;

				if (fileSize < 1000)
				{
					return fileSize.ToString("0") + "KB";
				}
				else
				{
					fileSize /= 1024;
					return fileSize.ToString("0.0") + "MB";
				}
			}

			return fileSize + "B";
		}

		private void upload()
		{
			EditorCoroutine.start(uploadImage());
		}

		private IEnumerator uploadImage()
		{
			if (getLastSaveFileSize() > 10000000) // 10MB
			{
				EditorUtility.DisplayDialog("Can't upload to imgur.com", "The gif you want to upload is larger than 10MB, it's " + getLastSaveFileSizeDescription() + "! This is the max size allowed by imgur.com. (You can try to reduce the output width to reduce the file size)", "Cancel Upload");
				yield break;
			}

			Dictionary<string, string> headers = new Dictionary<string, string> ();
			headers.Add("Authorization", "Client-ID " + "01c02b71cfa8334");

			WWWForm form = new WWWForm();
			form.AddField("type", "base64");
			form.AddField("image", Convert.ToBase64String(File.ReadAllBytes(lastSavedFilePath)));

			gifCreatorWindow.ProgressRenderer.UpdateProgress("Uploading (" + getLastSaveFileSizeDescription() + ") - this can take a few minutes...", 0.0f);

			WWW www = new WWW("https://api.imgur.com/3/image/", form.data, headers);

			while (!www.isDone)
			{
				yield return null;
			}

			if (!string.IsNullOrEmpty(www.error))
			{
				gifCreatorWindow.ProgressRenderer.UpdateProgress("Error uploading to imgur.com", 0.0f);
				Debug.LogError("An unknown error occured while uploading to imgur.com: " + www.error + "\n" + www.text);
			}
			else
			{
				JSONObject jsonObject = new JSONObject(www.text);
				bool success = jsonObject.GetField("success").b;

				if (!success)
				{
					string error = jsonObject.GetField("data").GetField("error").str;

					gifCreatorWindow.ProgressRenderer.UpdateProgress("Error uploading to imgur.com", 0.0f);
					Debug.LogError("An error occured while uploading to imgur.com: " + error + "\n" + www.text);
				}
				else
				{
					string url = jsonObject.GetField("data").GetField("link").str;
					url = url.Replace("\\", string.Empty);

					EditorGUIUtility.systemCopyBuffer = url;

					gifCreatorWindow.ProgressRenderer.UpdateProgress("Upload done (URL is in your clipboard)! See console for info", 1.0f);
					gifCreatorWindow.Repaint();

					Debug.Log("Your gif have successfully been uploaded to imgur.com (and copied to your clipboard): " + url);
				}
			}
		}

		private void drawWarningsAndErrors()
		{
			if (string.IsNullOrEmpty(gifCreatorWindow.Parameters.destinationFolder))
			{
				EditorGUILayout.HelpBox("You must specify a destination folder, otherwise the default destination '" + Application.dataPath + "' will be used", MessageType.Warning);
			}
		}
	}
}
