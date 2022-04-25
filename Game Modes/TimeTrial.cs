using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTrial : MonoBehaviour
{
    public static TimeTrial instance;
    public int[] mapTimes;

    void Awake()
	{
		if (instance == null) //Keeps only one instance in scene
		{
			instance = this;
		} else {
			Destroy(gameObject);
			return;
		}
	}

    public void playerTime(int playerTime)
    {
		
    }
}
