using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {
	public float MoveSpeed = 6.0f;

	Rigidbody rb;
	Camera viewCamera;

	Vector3 velocity;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody>();
		viewCamera = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 mousePos = viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, viewCamera.transform.position.y));
		transform.LookAt(mousePos + Vector3.up * transform.position.y);
		velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * MoveSpeed;
	}

	void FixedUpdate() {
		rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
	}
}
