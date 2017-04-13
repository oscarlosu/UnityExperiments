using UnityEditor;
using UnityEngine;
using PygmyMonkey.GifCreator.Utils;

namespace PygmyMonkey.GifCreator
{
	public class GifCreatorWindow : PMEditorWindow
	{
		public static string ProductName = "Gif Creator";
		public static string VersionName = "1.0.7";

		[MenuItem("Window/PygmyMonkey/Gif Creator")]
		private static void ShowWindow()
		{
			EditorWindow window = createWindow<GifCreatorWindow>(ProductName);
			window.minSize = new Vector2(375, 300);
		}

		public override string getProductName()
		{
			return ProductName;
		}

		public override string getVersionName()
		{
			return VersionName;
		}

		public override string getAssetStoreID()
		{
			return "42302";
		}

		/*
		* Data
		*/
		public RecorderParameters Parameters { get; private set; }

		/*
		* Renderers
		*/
		private EmptyRenderer emptyRenderer;
		public ParametersRenderer ParametersRenderer;
		public RecordRenderer RecordRenderer;
		public PreviewRenderer PreviewRenderer;
		public ExportRenderer ExportRenderer;
		public ProgressRenderer ProgressRenderer;

		/*
		* Init
		*/
		public override void init()
		{
			if (Parameters == null) Parameters = new RecorderParameters();

			if (emptyRenderer == null) emptyRenderer = new EmptyRenderer(this);
			if (ProgressRenderer == null) ProgressRenderer = new ProgressRenderer(this);
			if (ParametersRenderer == null) ParametersRenderer = new ParametersRenderer(this);
			if (RecordRenderer == null) RecordRenderer = new RecordRenderer(this);
			if (PreviewRenderer == null) PreviewRenderer = new PreviewRenderer(this);
			if (ExportRenderer == null) ExportRenderer = new ExportRenderer(this);

		}

		private bool isEditorPlaying = false;
		void Update()
		{
			emptyRenderer.Update();
			PreviewRenderer.Update();
			RecordRenderer.Update();
			ExportRenderer.Update();

			if (EditorApplication.isPlaying != isEditorPlaying)
			{
				isEditorPlaying = EditorApplication.isPlaying;

				emptyRenderer.OnPlayModeChange(isEditorPlaying);
				ParametersRenderer.OnPlayModeChange(isEditorPlaying);

				Parameters.UpdateGameViewSize();
			}
		}

		void OnDestroy()
		{
			emptyRenderer.OnDestroy();
		}

		/*
		* Drawing
		*/
		public override void drawContent()
		{
			emptyRenderer.drawInspector();

			if (Recorder.Singleton == null || CustomRectPreview.Singleton == null)
			{
				return;
			}

			ParametersRenderer.drawInspector();
			RecordRenderer.drawInspector();
			PreviewRenderer.drawInspector();
			ExportRenderer.drawInspector();
		}

		public void Reset()
		{
			if (ProgressRenderer != null)
			{
				ProgressRenderer.Hide();
			}

			if (ExportRenderer != null)
			{
				ExportRenderer.Reset();
			}

			if (Recorder.Singleton != null)
			{
				Recorder.Singleton.Reset();
			}
		}

		public override void drawEnd()
		{
			ProgressRenderer.drawInspector();
		}
	}
}