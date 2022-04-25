using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explode : MonoBehaviour 
{
	void OnEnable()
	{
		Invoke("Disable", 2f);
	}

	void Disable()
	{
		CancelInvoke();
		gameObject.SetActive(false);
	}
}
