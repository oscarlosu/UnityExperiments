using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonitorPhoneMotion : MonoBehaviour {
    public Vector3 acceleration;
    public Vector3 velocity;
    public bool accelerometer = false;

    public Vector3 angularVelocity;
    int touchCount;

    // Use this for initialization
    void Start () {
        Input.gyro.enabled = true;
    }
	
	// Update is called once per frame
	void Update () {
        touchCount = Input.touchCount;
        if(touchCount == 2 && Input.GetTouch(touchCount - 1).phase == TouchPhase.Began)
        {
            accelerometer = !accelerometer;
        }

        // Rotation (from https://forum.unity.com/threads/unity-and-the-accelerometer-vs-the-gyroscope-a-complete-guide.451496/)
        transform.rotation = Input.gyro.attitude;
        transform.Rotate(0f, 0f, 180f, Space.Self); // Swap "handedness" of quaternion from gyro.
        transform.Rotate(90f, 180f, 0f, Space.World); // Rotate to make sense as a camera pointing out the back of your device

        // Linear velocity
        acceleration = transform.TransformDirection(Input.gyro.userAcceleration);
        velocity += acceleration * Time.deltaTime;
        if (accelerometer)
        {
            transform.position += velocity * Time.deltaTime;
        }
        

    }

    private void OnGUI()
    {
        GUI.Label(new Rect(new Vector2(0, 0), new Vector2(100, 50)), new GUIContent("Acceleration " + acceleration.ToString()));
        GUI.Label(new Rect(new Vector2(0, 50), new Vector2(150, 50)), new GUIContent("Velocity " + velocity.ToString()));
        GUI.Label(new Rect(new Vector2(0, 100), new Vector2(150, 50)), new GUIContent("Touches " + touchCount.ToString()));
        GUI.Label(new Rect(new Vector2(0, 150), new Vector2(150, 50)), new GUIContent("Acceleration " + accelerometer));
    }
}
