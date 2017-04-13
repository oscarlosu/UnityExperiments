using UnityEditor;
using System.Collections;

// From: https://gist.github.com/benblo/10732554
public class EditorCoroutine
{
	public static EditorCoroutine start(IEnumerator routine)
	{
		EditorCoroutine coroutine = new EditorCoroutine(routine);
		coroutine.start();
		return coroutine;
	}

	private readonly IEnumerator routine;

	EditorCoroutine(IEnumerator routine)
	{
		this.routine = routine;
	}

	void start()
	{
		EditorApplication.update += update;
	}

	void stop()
	{
		EditorApplication.update -= update;
	}

	void update()
	{
		if (!routine.MoveNext())
		{
			stop();
		}
	}
}