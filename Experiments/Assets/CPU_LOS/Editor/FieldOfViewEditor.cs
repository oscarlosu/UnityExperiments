using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor {

	void OnSceneGUI() {
		FieldOfView fow = (FieldOfView)target;
		Handles.color = Color.white;

		Handles.DrawWireArc(fow.transform.position, Vector3.up, fow.transform.forward, -fow.viewAngle / 2, fow.viewRadius);
		Handles.DrawWireArc(fow.transform.position, Vector3.up, fow.transform.forward, fow.viewAngle / 2, fow.viewRadius);

		Vector3 viewAngleA = fow.DirFromAngle(-fow.viewAngle / 2, false);
		Vector3 viewAngleB = fow.DirFromAngle(fow.viewAngle / 2, false);
		Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleA * fow.viewRadius);
		Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleB * fow.viewRadius);

		foreach(Transform visibleTarget in fow.visibleTargets) {
			Handles.color = Color.red;
			Handles.DrawLine(fow.transform.position, visibleTarget.position);
		}


	}

}
