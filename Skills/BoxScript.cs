using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxScript : MonoBehaviour {

    [SerializeField] float amplitude = 0.5f;
    private Vector3 tempPos = new Vector3(0, 0, 0);

	void Start ()
    {
        //make platform adjust terrain rotation
        RaycastHit rcHit;
        //Make raycast direction down
        Vector3 theRay = transform.TransformDirection(Vector3.down);

        if (Physics.Raycast(transform.position, theRay, out rcHit, 5.0f, LayerMask.GetMask("Ground")))
        {
            //this is for getting distance from object to the ground
            float GroundDis = rcHit.distance;
            //with this you rotate object to adjust with terrain
            transform.rotation = Quaternion.FromToRotation(Vector3.up, rcHit.normal);
            //finally, this is for putting object over the ground
            transform.position = new Vector3(transform.position.x, (transform.position.y - GroundDis) + 1, transform.position.z);
            transform.eulerAngles = new Vector3 (transform.eulerAngles.x, 0.0f, transform.eulerAngles.z);
        }
	}
	
	void Update () 
    {
        FloatAnimation();
        transform.Rotate(new Vector3 (0, 1, 0) * Time.deltaTime * 20, Space.Self);
	}

    void FloatAnimation()
    {
        tempPos = new Vector3 (0, 0, 0);
        tempPos.y += Mathf.Sin(Mathf.PI * Time.fixedTime) * amplitude;
        transform.Translate(tempPos * Time.deltaTime);
    }

    void OnTriggerEnter (Collider collider)
    {
        if (collider.tag == "Player")
        {
            StartCoroutine("BoxReset");
            //add breaking animation here
        }
    }

    IEnumerator BoxReset() 
    {
        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSecondsRealtime(3);
        GetComponent<Collider>().enabled = true;
        GetComponent<MeshRenderer>().enabled = true;
    }
}
