using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingBomb : MonoBehaviour {

    private Rigidbody rb;
    public Transform bombModel;
    public float speed;

	void Awake ()
    {
        rb = GetComponent<Rigidbody>();
	}

	void Update ()
    {
        bombModel.transform.position = rb.position; //Keeps model same position as rigidbody
        bombModel.transform.Rotate(Vector3.right * speed * 0.25f); //Rotation
	}

    void FixedUpdate()
    {
        rb.velocity = new Vector3(transform.parent.forward.x * speed, rb.velocity.y, transform.parent.forward.z * speed); //Forward speed + keeping vertical velocity
        // rb.AddForce(Vector3.up * -60f, ForceMode.Acceleration); //Gravity
    }

    void OnCollisionEnter (Collision collider)
	{
		if (collider.gameObject.tag != "Ground" && collider.gameObject.tag != "NitroPad") //Destroys on collision with everything except ground/nitropads
		{
            // Instantiate(explosion, transform.position, Quaternion.identity);
            EffectsPooler.instance.getEffect(0, rb.position, null);
			transform.parent.gameObject.SetActive(false);
            //Add explosion effect here + player stun on collision with it (probably in player sript)
		}
	}

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "FallOff")
        {
            EffectsPooler.instance.getEffect(0, rb.position, null);
			transform.parent.gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
        rb.velocity = Vector3.zero;
        rb.transform.localPosition = Vector3.zero;
        bombModel.localPosition = Vector3.zero;
    }
}