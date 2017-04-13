using UnityEditor;
using UnityEngine;
using PygmyMonkey.GifCreator.Utils;

namespace PygmyMonkey.GifCreator
{
	public class ProgressRenderer
	{
		private GifCreatorWindow gifCreatorWindow;

		private string text;
		private float progress;

		private GUIStyle progressStyle;
		private bool display = false;

		public ProgressRenderer(GifCreatorWindow gifCreatorWindow)
		{
			this.gifCreatorWindow = gifCreatorWindow;
		}
		
		public void UpdateProgress(string text, float progress)
		{
			display = true;

			this.text = text;
			this.progress = progress;
		}

		public void Hide()
		{
			display = false;
		}

		public void drawInspector()
		{
			if (!display)
			{
				return;
			}

			if (progressStyle == null) progressStyle = new GUIStyle(EditorStyles.toolbar) { fixedHeight = 30 };

			using (new GUIUtils.GUIHorizontal(progressStyle))
			{
				Rect rect = EditorGUILayout.GetControlRect(false, progressStyle.fixedHeight - 10, GUILayout.Width(gifCreatorWindow.position.width - 10));
				rect.x += 3;
				rect.y += 3;
				rect.width -= 5;

				EditorGUI.ProgressBar(rect, progress, text + " - " + (progress * 100).ToString("00") + "%");
			}
		}
	}
}
