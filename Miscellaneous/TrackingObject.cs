using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingObject : MonoBehaviour {

    public GameObject trackingTarget;
    public float position = 0.5f;
    public float rotation = 0.5f;

	void Start () 
    {
        this.gameObject.transform.position = trackingTarget.transform.position;
	}
	
	void LateUpdate () 
    {
        this.gameObject.transform.position = Vector3.Slerp(this.gameObject.transform.position, trackingTarget.transform.position, position);
        this.gameObject.transform.rotation = Quaternion.Slerp(this.gameObject.transform.rotation, trackingTarget.transform.rotation, rotation);
	}
}
