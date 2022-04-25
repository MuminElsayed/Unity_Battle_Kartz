using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosionBottle : MonoBehaviour 
{
	private Rigidbody rb;
	[SerializeField]
	private float speed;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}
	
	void OnEnable()
	{
		rb.isKinematic = false;
		rb.velocity = transform.forward * speed;
	}

	void OnCollisionEnter (Collision collider)
	{
		if (collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
		{
			rb.isKinematic = true;
		} else {
			EffectsPooler.instance.getEffect(0, transform.position, null);
			gameObject.SetActive(false);
		}
	}

	void OnDisable()
	{
		rb.velocity = Vector3.zero;
	}
}