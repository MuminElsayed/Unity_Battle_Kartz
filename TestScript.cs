using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour {

	public GameObject gameController;
	public int kartIndex;
	public int racePosition;
	public int LapNumber;
	public int checkpointCounter;
	public float distanceToNextCheckpoint;

	void Update()
	{
		string distance = LapNumber.ToString("00") + checkpointCounter.ToString("000") + distanceToNextCheckpoint.ToString("0000.0");
		// gameController.GetComponent<GameController>().kartPositions(kartIndex, Mathf.RoundToInt(float.Parse(distance)));
	}
}
