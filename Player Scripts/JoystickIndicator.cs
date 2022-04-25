using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class JoystickIndicator : MonoBehaviour 
{
	private float moveHorizontal;
	private float moveVertical;
	public Transform kartModel;

	void Update()
	{
		moveHorizontal = Input.GetAxis("Horizontal") + CrossPlatformInputManager.GetAxis("Horizontal");
		moveVertical = Input.GetAxis("Vertical") + CrossPlatformInputManager.GetAxis("Vertical");

		transform.position = kartModel.position + kartModel.forward * (moveVertical * 4 - Mathf.Abs(moveHorizontal))+ transform.right * moveHorizontal * 2 + transform.up * 0.5f;
	}
}
