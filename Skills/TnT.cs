using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TnT : MonoBehaviour {

	private Rigidbody rb;
	[SerializeField]
	private float shootSpeed;
	private int groundLayer;
	private Animator animator;
	private int landHash = Animator.StringToHash("Land");
	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		groundLayer = LayerMask.NameToLayer("Ground");
		animator = rb.GetComponent<Animator>();
	}

	void Update()
	{
		//Gravity
		rb.AddForce(Vector3.down * shootSpeed * 4, ForceMode.Force);
	}

	void OnEnable()
	{
		rb.isKinematic = false;
		rb.velocity = transform.up * shootSpeed;
	}

	void OnCollisionEnter (Collision collider)
	{
		if (collider.gameObject.layer == groundLayer)
		{
			animator.SetBool(landHash, true);
			rb.isKinematic = true;
		} else {
			EffectsPooler.instance.getEffect(0, transform.position, null);
			animator.SetBool(landHash, false);
			gameObject.SetActive(false);
		}
	}

	void OnDisable()
	{
		rb.velocity = Vector3.zero;
		rb.isKinematic = false;
	}
}
