using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Script makes gameObject moves to a specified distance in front of it and then back to starting position
public class MovingEnemy : MonoBehaviour 
{
	public float speed;
	public float distanceToMove;

	private Vector3 startPos;
	private Vector3 endPos;


	void Start()
	{
		startPos = transform.position;
		endPos = transform.position + (transform.forward * distanceToMove);
	}

	void Update()
	{
		if (Vector3.Distance(transform.position, endPos) < 0.001f)
		{
			endPos = startPos;
			startPos = transform.position;
			// print("Reached Destination");
		} else {
			transform.position = Vector3.MoveTowards(transform.position, endPos, speed * Time.deltaTime);
			// print("Moving");
		}
	}
}
