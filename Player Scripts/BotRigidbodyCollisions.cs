using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotRigidbodyCollisions : MonoBehaviour {

	public GameObject kart;
    private Rigidbody rb;
    private float initialDrag;
    private BotController botController;

	// Use this for initialization
	void Start () 
	{
		rb = GetComponent<Rigidbody>();
        initialDrag = rb.drag;
        botController = kart.GetComponent<BotController>();
	}

    void OnCollisionEnter (Collision collision)
    {
        // if (collision.collider.tag == "PoisonBottle" || collision.collider.tag == "RollingBomb")
        // {
        //     // botController.startStun(0, 3); //Replaced with explosion object
        // }
        if (collision.collider.tag == "Ground")
        {
            rb.drag = initialDrag;
        } else if (collision.collider.tag == "OffGround")
        {
            rb.drag = initialDrag * 2;
        }
    }

    void OnTriggerEnter (Collider collider)
    {
        if (collider.tag == "PowerupBox")
        {
            if (botController.canPowerup == true)
            {
                botController.startGetSkill(); //gets skill then fires within the fn
                // Debug.Log("power up!");
            }
        } else if (collider.tag == "Checkpoint")
        {
            //add checkpoint fn
            botController.AddCheckpoint(int.Parse(collider.name)); //sends checkpoint number to the addCheckpoint fn
            // botController.lastCheckpointPos = collider.transform.position; //next checkpnt pos added to AddCheckpoint()
        } else if (collider.tag == "FallOff")
        {
            botController.Respawn();
        } else if (collider.tag == "TnT")
        {
            // botController.startStun(5, 3); //Replaced with explosion object
            collider.transform.parent = kart.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform; //Makes it child to kartModel
            collider.transform.localPosition = new Vector3(0, 2, -0.75f); //where the TNT lands on the player
        } else if (collider.tag == "Stun")
        {
            botController.startStun(0, 2); //fire/explosion stun
        } else if (collider.tag == "NitroPad")
        {
            botController.startNitroPadBoost(); //fire/explosion stun
        } else if (collider.tag == "BotJump")
        {
            botController.StartCoroutine("Jump");
        }
    }
}