using UnityEngine;
using ThreadPriority = System.Threading.ThreadPriority;

namespace PygmyMonkey.GifCreator
{
	[System.Serializable]
	public class RecorderParameters
	{
		public enum RecordMode
		{
			SINGLE_CAMERA,
			EVERYTHING,
		}

		public enum Section
		{
			Fullscreen,
			CustomRectAbsolute,
			CustomRectRelative,
		}

		public enum RecordType
		{
			Duration,
			LastSeconds,
		}

		public Camera camera;
		public RecordMode recordMode = RecordMode.SINGLE_CAMERA;
		public Section section;
		public float relativePosX = 0.0f;
		public float relativePosY = 0.0f;
		public float relativeWidth = 0.5f;
		public float relativeHeight = 0.5f;
		public int absolutePosX = 0;
		public int absolutePosY = 0;
		public int absoluteWidth = 320;
		public int absoluteHeight = 200;
		public Color customRectPreviewColor = new Color(1.0f, 0.0f, 0.0f, 0.2f);
		public bool showCustomRectPreview = false;
		public bool recordUpsideDown = false;
		public bool pauseAfterRecord = false;
		public RecordType recordType;
		public int framePerSecond = 15;
		public int repeat = 0;
		public int quality = 15;
		public float duration = 3f;
		public int recordWidth = 320;
		public ThreadPriority workerPriority = ThreadPriority.BelowNormal;
		public string destinationFolder;
		public bool openFolderAfterSave = false;
		public int outputWidth = 320;

		private System.Reflection.MethodInfo getSizeOfMainGameViewMethod;
		private Vector2 gameViewSize;

		public RecorderParameters()
		{
			System.Type gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
			getSizeOfMainGameViewMethod = gameViewType.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

			UpdateGameViewSize();

			#if UNITY_EDITOR
			recordUpsideDown = UnityEditor.EditorPrefs.GetBool("gifcreator_record_upsidedown", Application.platform == RuntimePlatform.OSXEditor);
			pauseAfterRecord = UnityEditor.EditorPrefs.GetBool("gifcreator_pause_after_record", false);
			framePerSecond = UnityEditor.EditorPrefs.GetInt("gifcreator_fps", 15);
			repeat = UnityEditor.EditorPrefs.GetInt("gifcreator_repeat", 0);
			quality = UnityEditor.EditorPrefs.GetInt("gifcreator_quality", 15);
			duration = UnityEditor.EditorPrefs.GetFloat("gifcreator_duration", 3f);
			destinationFolder = UnityEditor.EditorPrefs.GetString("gifcreator_destination_folder");
			openFolderAfterSave = UnityEditor.EditorPrefs.GetBool("gifcreator_open_after_save", false);
			#endif
		}

		public float getTimePerFrame()
		{
			return 1f / framePerSecond;
		}
		
		public Rect getRectSection()
		{
			switch (section)
			{
			case Section.CustomRectAbsolute:
				return new Rect(absolutePosX, absolutePosY, absoluteWidth, absoluteHeight);

			case Section.CustomRectRelative:
				return new Rect(relativePosX * getMainGameViewSize().x, relativePosY * getMainGameViewSize().y, relativeWidth * getMainGameViewSize().x, relativeHeight * getMainGameViewSize().y);
				
			default:
				return new Rect(0, 0, getMainGameViewSize().x, getMainGameViewSize().y);
			}
		}
		
		public float getEstimatedMemoryUse()
		{
			float mem = framePerSecond * duration;
			mem *= getRecordWidth() * getRecordHeight() * 4;
			mem /= 1024 * 1024;
			return mem;
		}

		public string getEstimatedMemoryUseDescription()
		{
			return framePerSecond + "fps * " + duration + " seconds * " + getRecordWidth() + " (width) * " + getRecordHeight() + " (height) * 4 (RGBA) / 1024 (to KB) / 1024 (to MB)";
		}

		public int getRecordWidth()
		{
			return recordWidth;
		}

		public int getRecordHeight()
		{
			return (int)(recordWidth * getRectSection().height / getRectSection().width);
		}

		public int getOutputWidth()
		{
			return outputWidth;
		}

		public int getOutputHeight()
		{
			return (int)(outputWidth * getRectSection().height / getRectSection().width);
		}

		public string getDestinationFolder()
		{
			if (string.IsNullOrEmpty(destinationFolder))
			{
				return Application.dataPath;
			}

			return destinationFolder;
		}

		public void UpdateGameViewSize()
		{
			System.Object resolution = getSizeOfMainGameViewMethod.Invoke(null,null);
			gameViewSize = (Vector2)resolution;
		}

		public Vector2 getMainGameViewSize()
		{
			return gameViewSize;
		}
	}
}