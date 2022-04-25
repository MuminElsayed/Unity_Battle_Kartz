using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;

public class GameController : MonoBehaviour {

	public static GameController instance;
	public bool debugging = true;

	[Header("Player References")]
	public GameObject playerInstance;
	public GameObject playerPrefab;
	public Texture playerTexture;
	public int numberOfPlayers;
	public List<GameObject> allPlayers;
	private List<float> distancesList;
	private List<float> sortedList;
	public GameObject[] playerPositions;
	[SerializeField]
	GameObject botPrefab;
	//-------------//
    private Vector3 playerOriginalPos;
	private Quaternion playerOriginalRot;
	
	//-------------//
	// [Header("Game Mode")]
	public enum gameModes
	{
		Classic, TimeTrial, Demolition, BattleArena
	}
	[SerializeField]
	public gameModes gameMode;

	[Header("Custom Skill Set")]
    
	[SerializeField]
	private Transform playerSpawnLocation;
	public GameObject checkpoints;
	public Transform[] allCheckpoints;
	public int numberOfLaps;
	public int numberOfBots;
	public int difficulty;
	public Coroutine raceStartCorou;
	[SerializeField]
	private GameObject selectedKart;
	public Vector3 botSpawnPos;

	[Header("Canvas References")]
	public string sceneToLoad;
	[SerializeField]
	private bool resetBtn;
	private Image startLight;
	
	//-------------//
	[Header("Sound Clips")]
	public AudioClip missileLaunch;
	
	[Header("Game Settings")]
	[SerializeField]
	private int defaultFPS = 60;
	//-------------//
	[Header("Temp")]
	public int targetPlayer;
	public Vector3 skillPos;
	public Vector3 skillRot;
	public int skillNumber;
	public int effectID;
	public Vector3 effectPos;

	void Awake()
	{
		if (instance == null) //Keeps only one instance in scene
		{
			instance = this;
		} else {
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject); //Stay alive between scenes

		//General game settings (graphics, volume, etc)
		Application.targetFrameRate = defaultFPS;
	}

		void Update () {
		
		//--Not used ingame/debugging--//

		if (Input.GetKeyDown("r")) 
		{
			resetPlayer();
		}

		if (Input.GetKeyDown("t")) 
		{
			//Respawns all bots
			foreach (GameObject bot in allPlayers)
			{
				if (bot.tag == "Bot")
				{
					bot.GetComponentInChildren<BotController>().Respawn();
				}
			}
		}

		if (Input.GetKeyDown("q"))
		{
			//Fires a skill
			Transform targetTransform = playerPositions[targetPlayer].transform;
			Vector3 targetPos = playerPositions[targetPlayer].transform.GetChild(1).transform.position + Vector3.up * 4;
			Vector3 targetForward = playerPositions[targetPlayer].transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).forward;
			Quaternion targetRot = playerPositions[targetPlayer].transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).rotation;
			// Skills.instance.getSkill(skillNumber, targetPos, targetRot.eulerAngles, targetTransform);
			Skills.instance.getSkill(skillNumber, skillPos, skillRot, targetTransform);
		}

		if (Input.GetKeyDown("e"))
		{
			//Uses an effect
			EffectsPooler.instance.getEffect(effectID, effectPos, null);
		}

		if (Input.GetKeyDown(KeyCode.F12))
		{
			//Take a screenshot in game main folder
			ScreenCapture.CaptureScreenshot("screenshot_" + Mathf.Round(Time.time) + ".png");
			print("screenshot_" + Mathf.Round(Time.time));
		}

		if (Input.GetKeyDown(KeyCode.Escape) && Time.timeScale != 0 && !endOfMatch.instance.gameObject.activeInHierarchy) //pauses the game
		{
			//Pause game
			Time.timeScale = 0f;
			playingCanvas.instance.gameObject.SetActive(false); //disables PlayingCanvas
			PauseMenu.instance.gameObject.SetActive(true); //enables pause menu
		}
		//------------------//
	}
	public void raceMapStart() //Gets called when the racing map starts
	{
		playerSpawnLocation = GameObject.Find("PlayerSpawnLocation").transform;
		//Used to spawn/reset player
		playerOriginalPos = playerSpawnLocation.position;
		playerOriginalRot = playerSpawnLocation.rotation;
		allPlayers = new List<GameObject>();

		try 
		{
			PauseMenu.instance.gameObject.SetActive(false);
			endOfMatch.instance.gameObject.SetActive(false);
			playingCanvas.instance.gameObject.SetActive(false);
		} catch {
			Debug.LogWarning("No racing canvases found");
		}

		//Gets and saves checkpoints positions
		setCheckpoints();

		if (playerSpawnLocation != null)
		{
			spawnPlayer();
		}

		try
		{
			allPlayers.Add(playerInstance); //Adds player to allPlayers list
		} catch {
			Debug.LogWarning("Player not found");
		}

		spawnBots(); //Spawns bots relative to player

		GameObject[] allBotsTag = GameObject.FindGameObjectsWithTag("Bot"); //Gets all spawned bots

		foreach(GameObject tagged in allBotsTag) //Filters children out -no children zone-
		{
			if(tagged.transform.parent == null)
			{
				allPlayers.Add(tagged);
			}
		}

		numberOfPlayers = allPlayers.Count; //Gets total number of players
		float[] tempArr = new float[numberOfPlayers]; //Creates temp array to resize distancesList
		distancesList = new List<float>(tempArr); //Resizes list to match numberOfPlayers
		sortedList = new List<float>(tempArr); //Resizes list to match numberOfPlayers

		for (int i = 0; i < numberOfPlayers; i++) //Gives each player a unique index and disables movement
		{
			//Seperate into 2 lists quicker
			try //Player
			{
				allPlayers[i].GetComponentInChildren<NewKartController>().kartIndex = i; //Script must be in children
			} catch { //Bot
				allPlayers[i].GetComponentInChildren<BotController>().kartIndex = i; //Script must be in children
			}
		}

		playerPositions = new GameObject[numberOfPlayers]; //Creates array that stores player ranks in the race

		startLight = playingCanvas.instance.transform.GetChild(0).GetComponent<Image>(); //Gets start Line light image

		setLapNumber();
		InvokeRepeating("updateKartPositions", 0f, 0.2f); //Updates all kart positions/ranks

		if (!debugging) //Not debugging/testing
		{
			raceStartCorou = StartCoroutine(raceStart()); //Starts race start logic
		} else {
			// CameraScript.instance.gameObject.GetComponentInChildren<Animator>().enabled = false;
			playingCanvas.instance.gameObject.SetActive(true);
		}
	}

	public void welcomeScreenStart()
	{
		// AudioManager.instance.PlaySound("Start Screen", 1f);
		setCheckpoints();
		botSpawnPos = allCheckpoints[0].position - allCheckpoints[0].forward * 10;
		allPlayers = new List<GameObject>(); //References race map needed variables
		//Spawns 4 bots
		for (int i = 0; i < 5; i++)
		{
			GameObject botInst = Instantiate(botPrefab, (allCheckpoints[0].position + allCheckpoints[0].right * 10) - allCheckpoints[0].forward * 20 - allCheckpoints[0].right * 5 * i, allCheckpoints[0].rotation);
			allPlayers.Add(botInst); //Adds to allPlayers list
			botInst.GetComponentInChildren<BotController>().kartIndex = i; //Sets kart index
			botInst.GetComponentInChildren<BotController>().numberOfLaps = 99; //Sets number of laps
		}
		//Getting/sorting player ranks
		numberOfPlayers = allPlayers.Count; //Gets total number of players
		float[] tempArr = new float[numberOfPlayers]; //Creates temp array to resize distancesList
		distancesList = new List<float>(tempArr); //Resizes list to match numberOfPlayers
		sortedList = new List<float>(tempArr); //Resizes list to match numberOfPlayers
		playerPositions = new GameObject[numberOfPlayers]; //Creates array that stores player ranks in the race
		InvokeRepeating("updateKartPositions", 1f, 1f); //Updates all kart positions/ranks
	}

	public void disruptSkill(int playerIndex, float time)
	{
		foreach (GameObject player in allPlayers)
		{
			if (player != allPlayers[playerIndex])
			{
				if (player == playerInstance)
				{
					player.GetComponentInChildren<NewKartController>().startDisrupt(time);
				} else {
					player.GetComponentInChildren<BotController>().startDisrupt(time);
				}
			}
		}
	}

	public void endMatch()
	{
		playingCanvas.instance.gameObject.SetActive(false); //removes both menu's
		PauseMenu.instance.gameObject.SetActive(false);
		endOfMatch.instance.gameObject.SetActive(true); //enables endmatch menu
	}

	public void kartPositions(int kartIndex, float distance, int botChecker) //Updates kart positions in race
	{
		distancesList[kartIndex] = distance; //Puts the player's distance in a unique spot in the list (by constant index)
		sortedList = new List<float>(distancesList); //Creates another distances list to sort
		sortedList.Sort(); //Sorts new list in ascending order
		int playerRank = numberOfPlayers - sortedList.IndexOf(distance); //Returns player rank depending on position in list, smallest value is last, highest first
		if (botChecker == 0) //Not a bot
		{
			allPlayers[kartIndex].GetComponentInChildren<NewKartController>().racePosition = playerRank; //Sends player rank back to kart
		} else if (botChecker == 1) { //A bot
			allPlayers[kartIndex].GetComponentInChildren<BotController>().racePosition = playerRank; //Sends player rank back to bot
		}
		playerPositions[playerRank - 1] = allPlayers[kartIndex]; //sets player rank from sorted list & allplayers
	}

	private IEnumerator raceStart() //Starts race countdown
	{
		foreach (GameObject player in allPlayers)
		{
			if (player == playerInstance) //Player
			{
				player.GetComponentInChildren<NewKartController>().canMove = false; //All players can't move
			} else { //Bot
				player.GetComponentInChildren<BotController>().canMove = false; //All players can't move
			}
		}
		//Time before race light starts -for transitions, animations-
		//Minimum time: 7.5f
		yield return new WaitForSeconds(7.5f);
		playingCanvas.instance.gameObject.SetActive(true);
		//Animation triggers here//

		for (int i = 1; i < 6; i++)
		{
			yield return new WaitForSeconds(1);
			// AudioManager.instance.PlaySound("Beep", 1f + i * 0.2f); //Play beep sounds
			startLight.fillAmount = i * 0.2f;
		}
		foreach (GameObject player in allPlayers)
		{
			if (player == playerInstance) //Player
			{
				player.GetComponentInChildren<NewKartController>().canMove = true; //All players can't move
			} else { //Bot
				player.GetComponentInChildren<BotController>().canMove = true; //All players can't move
			}
		}
		NewKartController.instance.startTimer();
		yield return new WaitForSeconds(2);
		//Play track music here
		startLight.gameObject.SetActive(false);
	}

	void updateKartPositions() //Gets all racers' positions
	{
		foreach (GameObject kart in allPlayers)
		{
			if (kart == playerInstance) //Player
			{
				kart.GetComponentInChildren<NewKartController>().UpdatePosition(); //Updates player pos
			} else { //Bot
				kart.GetComponentInChildren<BotController>().UpdatePosition(); //Updates bot pos
			}
		}
	}

	public void switchScenes(string sceneName) //For faster scenes (UI) Maybe add transitions here
	{
		Time.timeScale = 1f;
		SceneManager.LoadSceneAsync(sceneName);
	}

	void setLapNumber()
	{
		foreach (GameObject player in allPlayers)
		{
			if (player == playerInstance)
			{
				player.GetComponentInChildren<NewKartController>().numberOfLaps = numberOfLaps; //Sets lap number for all player
			} else {
				player.GetComponentInChildren<BotController>().numberOfLaps = numberOfLaps; //Sets lap number for all player
			}
		}
	}

	void spawnBots()
	{
		int Counter = 1;
		int positionStage = 0;
		for (int i = 1; i < numberOfBots + 1; i++)
		{
			//Spawn relative to player position
			Instantiate(botPrefab, playerInstance.transform.position - (playerInstance.transform.right * Counter * 7) - (playerInstance.transform.forward * positionStage * 7), playerInstance.transform.GetChild(0).rotation);
			Counter += 1;
			if (Counter == 4)
			{
				positionStage += 1;
				Counter = 0;
			}
		}
	}

	public void resetPlayer()
	{
		playerInstance.GetComponentInChildren<Rigidbody>().transform.position = playerOriginalPos;
		playerInstance.GetComponentInChildren<Rigidbody>().velocity = new Vector3 (0,0,0);
		playerInstance.GetComponentInChildren<Rigidbody>().angularVelocity = new Vector3 (0,0,0);
		playerInstance.transform.GetChild(0).rotation = playerOriginalRot;
	}

	private void spawnPlayer()
	{
		playerInstance = Instantiate(playerPrefab, playerOriginalPos, playerOriginalRot);
		playerInstance.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Renderer>().material.SetTexture("_BaseMap", playerTexture); //Changes kart's base texture
	}

	public void setCheckpoints()
	{
		//Gets and saves checkpoints positions
		checkpoints = GameObject.Find("Checkpoints");
		allCheckpoints = new Transform[checkpoints.transform.childCount]; 
		for (int i = 0; i < checkpoints.transform.childCount; i++)
		{
			allCheckpoints[i] = checkpoints.transform.GetChild(i);
		}
	}
}