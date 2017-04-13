using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour {
    public Transform center;
    public float speed;
    
    void Update() {
        transform.RotateAround(center.position, center.up, speed * Time.deltaTime);
    }
 
}
