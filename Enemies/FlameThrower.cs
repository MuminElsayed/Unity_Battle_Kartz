using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameThrower : MonoBehaviour 
{
	void Start()
	{
		InvokeRepeating("switchActiveStates", 0, 3f);
	}

	void switchActiveStates()
	{
		if (gameObject.activeSelf == true)
		{
			gameObject.SetActive(false);
		} else {
			gameObject.SetActive(true);
		}
	}

	void OnTriggerEnter (Collider collider)
	{
		// print("fire!!!!!");
		// Burn player here =)
	}
}