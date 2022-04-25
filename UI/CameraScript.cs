 using UnityEngine;
 using System.Collections;
 
//  [ExecuteInEditMode]
  public class CameraScript : MonoBehaviour
 {
	public static CameraScript instance;
    public Transform player;
	[SerializeField]
	private bool targetPlayer = true;
	public Animator anim;

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

	void Start()
	{
		anim = transform.GetChild(0).GetComponent<Animator>();
		if (targetPlayer) //Not debugging
		{
			player = GameController.instance.playerInstance.transform.GetChild(0); //Gets summoned player from gameController
		} else { //Target Bot
			player = GameObject.Find("Bot(Clone)").transform.GetChild(0);
		}
	}

	void LateUpdate ()
	{
		if (player == null)
		{
			player = GameController.instance.allPlayers[0].transform;
		}
		transform.position = player.position;
		transform.forward = player.forward;
	}
 }