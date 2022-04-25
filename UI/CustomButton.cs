using System.Collections;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class CustomButton : MonoBehaviour 
{
	public string AxisName;
	public float AxisValue;

	void Update()
	{
		
	}

	public void OnTouch()
	{
		CrossPlatformInputManager.SetAxis(AxisName, AxisValue);
		// print(CrossPlatformInputManager.GetAxis(AxisName));
	}

	public void OnOut()
	{
		CrossPlatformInputManager.SetAxis(AxisName, 0);
		// print(CrossPlatformInputManager.GetAxis(AxisName));
	}
}
