using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSkull : MonoBehaviour 
{
	public float speed;
	public GameObject[] targets;
	public int playerRank;
	private Rigidbody rb;
	public int targetCount;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	void OnEnable()
	{
		playerRank = 1;
		targetCount = 0;
		if (transform.parent!= null) //There's a parent
		{
			if (transform.parent.GetComponentInChildren<NewKartController>() != null) //Parent is player
			{
				playerRank = transform.parent.GetComponentInChildren<NewKartController>().racePosition;
			} else { //Parent is bot
				playerRank = transform.parent.GetComponentInChildren<BotController>().racePosition;
			}
		}
		// targets = gameController.GetComponent<GameController>().playerPositions;
		targets = new GameObject[playerRank - 1];
		
		for (int i = 1; i < playerRank; i++)
		{
			targets[i - 1] = GameController.instance.playerPositions[playerRank - 1 - i]; //gets inverted ranks after player (3rd, 2nd, 1st.. etc)
		}
		if (playerRank == 1) //Switches to no target mode with collisions
		{
			transform.forward = transform.parent.GetChild(0).forward;
		}
	}

	void Update()
	{
		if (playerRank != 1) //if player is not first (no targets)
		{
			transform.LookAt(targets[targetCount].transform.GetChild(1).transform);
		}
	}

	void FixedUpdate()
	{
		rb.velocity = transform.forward * speed;
	}

	void OnTriggerEnter(Collider collider)
	{
		if (playerRank != 1) //checks if there are targets
		{
			if (collider.transform.parent != null) //Check if there's a parent
			{
				if (collider.transform.parent.name == targets[targetCount].name) //Problem if no parent
				{
					if (targetCount != playerRank - 2)
					{
						targetCount += 1;
					} else {
						gameObject.SetActive(false);
					}
				}
			}
		}
	}
}
