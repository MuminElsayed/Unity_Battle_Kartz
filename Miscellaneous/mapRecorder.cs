using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Recorder;

public class mapRecorder : MonoBehaviour 
{
    public int nextCheckpoint = 0;
    public float speed;
    public float rotationSpeed;
    void Start()
    {
        //Starts at first checkpoint
        transform.position = GameController.instance.allCheckpoints[0].position;
        transform.rotation = GameController.instance.allCheckpoints[0].rotation;
    }

    void Update()
    {
        //Sets movement
        transform.position = Vector3.MoveTowards(transform.position, GameController.instance.allCheckpoints[nextCheckpoint].position, speed * Time.deltaTime);
        //Sets rotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, GameController.instance.allCheckpoints[nextCheckpoint].rotation, Time.deltaTime * rotationSpeed);
    }
    
    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Checkpoint")
        {
            nextCheckpoint += 1;
            if (nextCheckpoint == GameController.instance.allCheckpoints.Length)
            {
                nextCheckpoint = 0;
                print("Done Lap");
                //Stop recording here
            }
        }
    }
}