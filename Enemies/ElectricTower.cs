using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricTower : MonoBehaviour 
{
	// private GameObject children;

	// void Start ()
	// {
	// 	children = GetComponentInChildren<GameObject>();
	// }
	void OnTriggerEnter ()
	{
		StartCoroutine("Shock");
	}

	private IEnumerator Shock()
	{
		transform.GetChild(0).gameObject.SetActive(true);
		yield return new WaitForSeconds(1f);
		transform.GetChild(0).gameObject.SetActive(false);
	}
}
