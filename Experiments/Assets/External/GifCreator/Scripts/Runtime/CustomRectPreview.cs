using UnityEngine;
using System.Collections;

namespace PygmyMonkey.GifCreator
{
	[ExecuteInEditMode]
	public class CustomRectPreview : MonoBehaviour
	{
		public static CustomRectPreview Singleton;

		private Material lineMaterial;
		private RecorderParameters parameters;

		private float startX;
		private float startY;
		private float endX;
		private float endY;

		private static System.Reflection.MethodInfo getMainGameViewMethod;

		private bool show = false;
		
		void Awake()
		{
			initGameViewMethod();
		}

		public void OnEnable()
		{
			Singleton = this;
		}

		private void createLineMaterial()
		{
			// Unity has a built-in shader that is useful for drawing simple colored things.
			lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
		}

		private void initGameViewMethod()
		{
			#if UNITY_EDITOR
			System.Type gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
			getMainGameViewMethod = gameViewType.GetMethod("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			#endif
		}
		
		public void Show(RecorderParameters parameters)
		{
			this.parameters = parameters;

			show = true;
			repaintGameView();
		}

		public void Hide()
		{
			show = false;
			repaintGameView();
		}

		// Will be called after all regular rendering is done
		void OnRenderObject()
		{
			if (!show)
			{
				return;
			}

			if (lineMaterial == null)
			{
				createLineMaterial();
			}

			if (parameters.section == RecorderParameters.Section.CustomRectAbsolute)
			{
				startX = parameters.absolutePosX / parameters.getMainGameViewSize().x;
				startY = parameters.absolutePosY / parameters.getMainGameViewSize().y;
				endX = (parameters.absolutePosX + parameters.absoluteWidth) / parameters.getMainGameViewSize().x;
				endY = (parameters.absolutePosY + parameters.absoluteHeight) / parameters.getMainGameViewSize().y;
			}
			else if (parameters.section == RecorderParameters.Section.CustomRectRelative)
			{
				startX = parameters.relativePosX;
				startY = parameters.relativePosY;
				endX = parameters.relativePosX + parameters.relativeWidth;
				endY = parameters.relativePosY + parameters.relativeHeight;
			} 

			GL.PushMatrix();
			{
				lineMaterial.SetPass(0);

				GL.LoadOrtho();
				GL.Begin(GL.QUADS);
				{
					GL.Color(parameters.customRectPreviewColor);
					GL.Vertex3(startX, startY, 0);
					GL.Vertex3(startX, endY, 0);
					GL.Vertex3(endX, endY, 0);
					GL.Vertex3(endX, startY, 0);
				}
				GL.End();
			}
			GL.PopMatrix();
		}

		private void repaintGameView()
		{
			#if UNITY_EDITOR
			if (getMainGameViewMethod == null)
			{
				initGameViewMethod();
			}

			UnityEditor.EditorWindow gameviewWindow = (UnityEditor.EditorWindow)getMainGameViewMethod.Invoke(null,null);
			if (gameviewWindow != null)
			{
				gameviewWindow.Repaint();
			}
			#endif
		}
	}
	
}