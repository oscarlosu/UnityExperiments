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


    void Start () {
		
	}
	
	void Update () {
		
	}
}
