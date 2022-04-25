using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class makeInvisible : MonoBehaviour 
{
	void Start() //Removes renderer in all children on start
	{
		Renderer[] objects = transform.GetComponents<Renderer>();

		objects = transform.GetComponentsInChildren<Renderer>();
		
		foreach (Renderer item in objects)
		{
			item.enabled = false;
		}
	}

}
