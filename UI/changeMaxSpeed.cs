using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class changeMaxSpeed : MonoBehaviour {

	private Text maxSpeedText;

	void Start()
	{
		maxSpeedText = this.GetComponent<Text>();
	}

	public void changeSpeed(float newMaxspeed)
	{
		maxSpeedText.text = "Change Max Speed! (Current: " + newMaxspeed.ToString() + ")";
	}
}
