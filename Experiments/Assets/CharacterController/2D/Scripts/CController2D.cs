using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CController2D : MonoBehaviour {
    protected Rigidbody2D rb;
    protected UserInput input;
    [SerializeField]
    [Range(0.0f, Mathf.Infinity)]
    protected float g = 9.8f;
    [SerializeField]
    [Range(0.0f, Mathf.Infinity)]
    protected float drag = 5.0f;
    [SerializeField]
    protected Vector2 maxAcc = Vector2.one * 10.0f;
    [SerializeField]
    protected Vector2 a = Vector2.zero;

    protected Vector2 v = Vector2.zero;

    void Start () {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<UserInput>();
    }
	
	void FixedUpdate () {
        // TODO: Cache constant accelerations multiplied by Time.fixedDeltaTime


        // Guard against missing references
        if(rb == null || input == null) {
            Debug.LogWarning(this.GetType().Name + " couldn't find necessary components.");
            return;
        }


        // Acceleration user input
        a = Vector2.Scale(maxAcc, input.XYInput);
        v += a * Time.fixedDeltaTime;
        // Drag
        // Note 1: Drag always applies in the opposite sense of the current velocity
        // Note 2: Drag can never change the sign of the current velocity
        float vxSignum = Mathf.Sign(v.x);
        float vySignum = Mathf.Sign(v.y);
        v.x = v.x - vxSignum * drag * Time.fixedDeltaTime;
        v.x = vxSignum == Mathf.Sign(v.x) ? v.x : 0;
        v.y = v.y - vySignum * drag * Time.fixedDeltaTime;
        v.y = vySignum == Mathf.Sign(v.y) ? v.y : 0;

        // Gravity
        // TODO: Should drag apply to gravity?
        v.y -= g * Time.fixedDeltaTime;

        // Restrict movement with obstacles
        RaycastHit2D[] hits = new RaycastHit2D[1];
        // Cast rigidbody to anticipate collisions
        if(rb.Cast(v, hits, v.magnitude) > 0) {
            Vector2 vs, vn;
            foreach(RaycastHit2D h in hits) {
                // Split velocity in surface and surface normal components
                vn = Vector2.Dot(h.normal, v) * h.normal;
                vs = v - vn;
                // Reduce vn magnitude with distance to obstacle
                vn = vn.normalized * Mathf.Min(h.distance / Time.fixedDeltaTime, v.magnitude);
                v = vs + vn;
            }            
        }
        // Move rigidbody using velocity
        rb.MovePosition(rb.position + Time.fixedDeltaTime * v);
	}

}
