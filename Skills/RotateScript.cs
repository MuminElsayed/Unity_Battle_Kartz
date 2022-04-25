using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateScript : MonoBehaviour {

	public float rotateSpeed;
	
	void Update () 
    {
        transform.Rotate(new Vector3 (0, 1, 0) * Time.deltaTime * rotateSpeed, Space.Self);
	}
}
