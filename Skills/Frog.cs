using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frog : MonoBehaviour 
{
	private Rigidbody rb;
	[SerializeField]
	private float speed;
    private AudioSource audioSource;
	private int groundLayer;
	private Animator animator;
	private int landHash = Animator.StringToHash("Land");

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
        audioSource = GetComponentInChildren<AudioSource>();
		groundLayer = LayerMask.NameToLayer("Ground");
	}
	
	void OnEnable()
	{
		rb.isKinematic = false;
		rb.velocity = transform.forward * speed;
	}

	void OnCollisionEnter (Collision collider)
	{
		if (collider.gameObject.layer == groundLayer)
		{
			rb.isKinematic = true;
			animator.SetBool(landHash, true);
		} else {
			EffectsPooler.instance.getEffect(0, transform.position, null);
			gameObject.SetActive(false);
		}
	}

    void OnTriggerEnter (Collider collider)
    {
		if (!audioSource.isPlaying)
		{
			audioSource.Play();
		}
    }

	void OnDisable()
	{
		animator.SetBool(landHash, false);
		rb.velocity = Vector3.zero;
	}
}