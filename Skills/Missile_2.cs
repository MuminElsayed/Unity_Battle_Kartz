using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile_2 : MonoBehaviour {

	private Rigidbody rb;
	[SerializeField]
	private float speed, rotateSpeed, lifetime;
	[SerializeField]
	private Transform target;
	private int playerRank, groundLayer;

	void Awake()
	{
		rb = GetComponent<Rigidbody>();
		groundLayer = LayerMask.NameToLayer("Ground");
	}
	void OnEnable () 
	{
		Invoke("Disable", lifetime); //Disables after set time
		if(transform.parent.GetComponentInChildren<NewKartController>() != null) //Player
		{
			playerRank = transform.parent.GetComponentInChildren<NewKartController>().racePosition; //Gets player rank from parent
		} else { //bot
			playerRank = transform.parent.GetComponentInChildren<BotController>().racePosition; //Gets player rank from parent
		}
		if (playerRank != 1) //Player is not first, set target
		{
			target = GameController.instance.playerPositions[playerRank - 2].transform.GetChild(0); //Targets the rigidbody not parent	
			transform.rotation = Quaternion.LookRotation(target.position - transform.position); //Looks at target
		}
	}

	void Update () 
	{
		if (target != null) //There is a target, rotate towards it
		{
			Quaternion targetRot = Quaternion.LookRotation(target.position - transform.position);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, Time.deltaTime * rotateSpeed);
		}
	}

	void FixedUpdate()
	{
		rb.velocity = transform.forward * speed; //Fixed speed
	}

	void OnCollisionEnter (Collision collider)
	{
		if (collider.gameObject.layer != groundLayer)
		{
			Disable();
		}

	}

	void Disable()
	{
		// Instantiate(explosion, transform.position, Quaternion.identity);
		EffectsPooler.instance.getEffect(0, transform.position, null);
		target = null;
		transform.position = Vector3.zero;
		CancelInvoke();
		gameObject.SetActive(false);
	}
}