using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour 
{
	public GameObject endPos;
	public static bool canTp = true;

	void OnTriggerEnter (Collider collider)
	{
		StartCoroutine(teleport(collider));
	}

	IEnumerator teleport(Collider collider)
	{
		if (canTp)
		{
			collider.transform.position = endPos.transform.position;
			canTp = false;
		}
		yield return new WaitForSeconds(1);
		canTp = true;
	}
}
