using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class endOfMatch : MonoBehaviour {

	public static endOfMatch instance;

	void Awake()
	{
		if (instance == null)
		{
			instance = this;
		} else {
			Destroy(gameObject);
			return;
		}
	}
}
