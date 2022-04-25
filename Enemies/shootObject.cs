using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shootObject : MonoBehaviour 
{
	[SerializeField]
	private float spawnTime;
	[SerializeField]
	private GameObject spawnLocation;
	[SerializeField]
	private int skillID;

	void Start()
	{
		InvokeRepeating("Spawner", spawnTime, spawnTime);
	}

	void Spawner()
	{
		Skills.instance.getSkill(skillID, spawnLocation.transform.position, spawnLocation.transform.forward, transform);
	}

}
