using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : MonoBehaviour {
    [SerializeField]
    [Range(-1.0f, 1.0f)]
    protected float xInput;
    public float XInput {
        get {
            return xInput;
        }
        set {
            xInput = Mathf.Clamp(value, 0.0f, 1.0f);
        }
    }
    [SerializeField]
    [Range(-1.0f, 1.0f)]
    protected float yInput;
    public float YInput {
        get {
            return yInput;
        }
        set {
            yInput = Mathf.Clamp(value, 0.0f, 1.0f);
        }
    }

    public Vector2 XYInput {
        get {
            return new Vector2(XInput, YInput);
        }
        set {
            XInput = value.x;
            YInput = value.y;
        }
    }


    void Start () {
		
	}
	
	void Update () {
		
	}
}
