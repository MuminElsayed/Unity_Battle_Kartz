using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyCollisions : MonoBehaviour {

	public GameObject kart;
    private Rigidbody rb;
    private float initialDrag;
    private NewKartController kartControllerScript;

	// Use this for initialization
	void Start () 
	{
		rb = GetComponent<Rigidbody>();
        initialDrag = rb.drag;
        kartControllerScript = kart.GetComponent<NewKartController>();
	}
    void OnCollisionEnter (Collision collision)
    {
        // if (collision.collider.tag == "PoisonBottle" || collision.collider.tag == "RollingBomb")
        // {
        //     // kartControllerScript.startStun(0, 3); //Replaced with explosion object
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
            if (kartControllerScript.canPowerup == true)
            {
                kartControllerScript.StartCoroutine("getSkill"); //Change
                // Debug.Log("power up!");
            }
        } else if (collider.tag == "PowerOrb")
        {
            kartControllerScript.addPowerOrb(1);
        } else if (collider.tag == "PowerOrbBox")
        {
            kartControllerScript.addPowerOrb(7);
        } else if (collider.tag == "Checkpoint")
        {
            //add checkpoint fn
            kartControllerScript.AddCheckpoint(int.Parse(collider.name)); //sends checkpoint number to the addCheckpoint fn

        } else if (collider.tag == "FallOff")
        {
            kartControllerScript.Respawn();
        } else if (collider.tag == "TnT")
        {
            // kartControllerScript.startStun(5, 3); //Replaced with explosion object
            collider.transform.parent = kart.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform; //Makes it child to kartModel
            collider.transform.localPosition = new Vector3(0, 2, -0.75f); //where the TNT lands on the player
        } else if (collider.tag == "Stun")
        {
            kartControllerScript.startStun(0, 2); //fire/explosion stun
        } else if (collider.tag == "NitroPad")
        {
            kartControllerScript.startNitroPadBoost();
        }
    }
}