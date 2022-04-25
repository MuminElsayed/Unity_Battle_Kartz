using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickToGround : MonoBehaviour {

    public float distance = 1f;

    private Rigidbody rb;
    private Quaternion targetQuat;
    private float rotation = 0.25f;

    void Start () 
    {
        rb = GetComponent<Rigidbody>();
	}
	
	void Update () 
    {
		
	}

    void FixedUpdate ()
    {
        TouchingTheGround();
    }

    void TouchingTheGround () 
    {
        Vector3 position = transform.position + transform.TransformDirection(Vector3.up) * 0.5f;
        Vector3 direction = transform.TransformDirection (Vector3.down);
        Ray ray = new Ray (position, direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distance, LayerMask.GetMask("Ground")))
        {
            Vector3 hitUpVector = hit.normal;

            Vector3 fPosition = transform.position + transform.TransformDirection(new Vector3(0.0f, 2.0f, 1.0f));
            Vector3 fDirection = transform.TransformDirection(Vector3.down);
            Ray fRay = new Ray(fPosition, fDirection);
            RaycastHit fHit;
            float fDistance = distance * 3;

            rb.transform.position = hit.point;

            if (Physics.Raycast(fRay, out fHit, fDistance, LayerMask.GetMask("Ground")))
            {
                targetQuat.SetLookRotation(fHit.point - transform.position, hitUpVector);
            }

            rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, targetQuat, rotation);
        }
    }
}
