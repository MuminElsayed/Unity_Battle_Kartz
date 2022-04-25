using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.Events;
using System.Diagnostics;
using TMPro;

public class NewKartController : MonoBehaviour {
    public static NewKartController instance;

    [Header("Kart Stats")]

    [SerializeField]
    private float forwardSpeed;
    public float steerAngle, driftingSteerAngle;
    [SerializeField]
	private float detectionDistance = 2f, maxSpeed, jumpPower = 10, jumpTime, boostTime = 6f, gravity, nitroBonusTime;
    private float nitroBonusSpeed;
    private float prevNitroBonusSpeed = 0;
    private float initialDrag;
    private float joystickDeadzone = 0.3f;
    //----------------//

    private float nitroExtraBoost;
    private int playerSpeed;
    private float defaultForwardSpeed;
    private float defaultDetectionDistance;
    private float defaultMaxspeed;
    private float defaultSteerAngle;
    private Vector2 stickInput;

    [Header("Kart Statuses")]
    
    private int powerOrbCount = 0;
    private bool jumpBtn;
    private bool driftRight = false;
    private bool driftLeft = false;
    private int driftStage = 0;
    private float driftPower = 0;
    [SerializeField]
    private bool onGround = false;
    public int racePosition, kartIndex, numberOfLaps;
    private int nextCheckpoint = 1;
    [SerializeField]
    private float distanceFromNextChkPnt, playerPositions;
    private Vector3 nextCheckpointPos;
    private int landSpeed;
    public bool canMove = true;

    //----------------//

    private bool jumping = false;
    private bool drifting = false;
    private bool canBoost = false;
    private bool nitro;
    private bool nitroPadBoost = false;
    public bool canUseSkill = false;
    private bool canJump = true;
    private bool canBeStunned = true;
    private GameObject createdShield;
    private GameObject createdInvincible;
    private GameObject createdPoweredUp;
    private GameObject createdDisruptedEffect;
    private Coroutine shieldCoroutine;
    private Coroutine invincibleCoroutine;
    private Coroutine poweredUpCoroutine;
    private Coroutine disruptedCoroutine;
    private Coroutine nitroCoroutine;
    private Coroutine nitropadBoostCoroutine;
    private bool handbrake;
    public int LapNumber = 0;
    [SerializeField]
    private int lastCheckpoint = 1, numberOfCheckpoints;

    [Header("Kart References")]
    
    public bool canPowerup = true;
	private LayerMask groundLayerMask;
    private Text speedoMeter, kartStatus;
    private TextMeshProUGUI powerOrbCounter, lapCount, lapTimer, lapTimesFinal, racePosText, finalRankText, driftPowerText;
    // public Text lapTimesFinal;
    public string[] lapTimes;
    public int timeElapsed;
    private Image driftBar, powerupImage;
    public Sprite[] powerupsSprites;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Material normalMat, nitroMat, canBoostMat;
    [SerializeField]
    private GameObject skidMarks;
    private float lapStartTime;
    
    //----------------//

    private ParticleSystem[] ps;
    private Transform kartNormal;
	private Transform kartModel;
    private Rigidbody rigidBody;
    [SerializeField]
    private Vector3 LastOngroundRotation;
    private float moveHorizontal;
    private float moveHorizontal_2;
    private float moveVertical;
    private float moveVertical_2;
    private bool onGroundTag;
    public int skillNumber;
    private float jumpTimeStart;
    private float jumpTimeEnd;
    private Quaternion playerRotation = Quaternion.identity;
    private int numberOfPlayers;
    private AudioSource audioSrc;

    [Header("Forward Skills Prefabs")]
    [SerializeField]
    private GameObject ShieldPrefab, InvinciblePrefab, poweredUpPrefab, slowedEffectPrefab;
    
    [Header("Skillsets")]
    [SerializeField]
    private int[] WinningSkillSet, LosingSkillSet; //Takes a defined set of skills from skillPooler by array order

    [Header("Others")]
    private Renderer psRenderer1;
    private Renderer psRenderer2;
    private int horizontalHash = Animator.StringToHash("Horizontal");
    private int verticalHash = Animator.StringToHash("Vertical");
    private int wheelsDirectionHash = Animator.StringToHash("WheelsDirection");
    private int driftDirectionHash = Animator.StringToHash("DriftDirection");

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


    void Start () 
    {
        //Assigning all text variables
        speedoMeter = playingCanvas.instance.speedoMeter;
        kartStatus = playingCanvas.instance.kartStatus;
        powerOrbCounter = playingCanvas.instance.powerOrbCounter;
        lapCount = playingCanvas.instance.lapCount;
        lapTimer = playingCanvas.instance.lapTimer;
        lapTimesFinal = playingCanvas.instance.lapTimesFinal;
        driftPowerText = playingCanvas.instance.driftPowerText;
        racePosText = playingCanvas.instance.racePosText;
        finalRankText = playingCanvas.instance.finalRankText;
        driftBar = playingCanvas.instance.driftBar;
        powerupImage = playingCanvas.instance.powerupImage;
        powerupsSprites = new Sprite[Skills.instance.allSkills.Length];
        for (int i = 0; i < Skills.instance.allSkills.Length; i++)
        {
            powerupsSprites[i] = Skills.instance.allSkills[i].objectSprite;
        }
        kartNormal = transform.GetChild(0);
        kartModel = transform.GetChild(0).GetChild(0).GetChild(0);
        rigidBody = transform.parent.GetChild(1).GetComponent<Rigidbody>();
        rigidBody = rigidBody.GetComponent<Rigidbody>();
        initialDrag = rigidBody.drag;
        InvokeRepeating("UpdatePlayerSpeed", 0f, 0.5f);
        defaultForwardSpeed = forwardSpeed;
        defaultDetectionDistance = detectionDistance;
		groundLayerMask = LayerMask.GetMask("Ground");
        defaultMaxspeed = maxSpeed;
        numberOfCheckpoints = GameController.instance.checkpoints.transform.childCount;
        lapCount.text = "LAP: " + 1 + "/" + numberOfLaps; //Sets the lap counter text
        lapTimes = new string[numberOfLaps];
        defaultSteerAngle = steerAngle;
        resetMaxSpeed();
        nextCheckpointPos = GameController.instance.allCheckpoints[0].position; //Sets first checkpoint to start pos
        numberOfPlayers = GameController.instance.numberOfPlayers;
        ps = GetComponentsInChildren<ParticleSystem>();
        psRenderer1 = ps[0].GetComponent<Renderer>();
        psRenderer2 = ps[1].GetComponent<Renderer>();
        audioSrc = GetComponent<AudioSource>();
    }

    void Update () 
    {
        UpdatePlayerSpeed();
        updateKartSound();
        //Unoptimized -- don't set material every frame
        if (canBoost == true)
        {
            psRenderer1.material = canBoostMat;
            psRenderer2.material = canBoostMat;
            CameraScript.instance.anim.SetBool("Nitro", false);
        } else if (nitro == true){
            psRenderer1.material = nitroMat;
            psRenderer2.material = nitroMat;
            CameraScript.instance.anim.SetBool("Nitro", true);
        } else {
            psRenderer1.material = normalMat;
            psRenderer2.material = normalMat;
            CameraScript.instance.anim.SetBool("Nitro", false);
        }

        moveHorizontal = CrossPlatformInputManager.GetAxis ("Horizontal") + Input.GetAxisRaw ("Horizontal");
        moveHorizontal_2 = CrossPlatformInputManager.GetAxis ("Horizontal_2") + Input.GetAxis ("Horizontal_2");
        moveVertical_2 = CrossPlatformInputManager.GetAxis ("Vertical_2") + Input.GetAxis ("Vertical_2");

        
        
        // stickInput = new Vector2(moveHorizontal, moveVertical); //Deadzone making axis not reaching max value - disabled for now
        // if (stickInput.magnitude < joystickDeadzone)
        // {
        //     stickInput = Vector2.zero;
        // } else
        // {
        //     stickInput = stickInput.normalized * ((stickInput.magnitude - joystickDeadzone) / (1 - joystickDeadzone));	
        // }
        // moveHorizontal = stickInput.x;
        // moveVertical = stickInput.y;

        // if (moveVertical < 10 && moveVertical > -4)
        // {
        //     moveVertical += (Input.GetAxisRaw("Vertical") + CrossPlatformInputManager.GetAxis("Vertical")) * Time.deltaTime * 6f;
        // } 

        //Manual Acceleration
        // if (Input.GetAxisRaw("Vertical") > 0 || CrossPlatformInputManager.GetAxis("Vertical") > 0)
        // {
        //     moveVertical = Mathf.SmoothStep(moveVertical, 10, Time.deltaTime * 8f);
        // } else if (Input.GetAxisRaw("Vertical") < 0 || CrossPlatformInputManager.GetAxis("Vertical") < 0)
        // {
        //     moveVertical = Mathf.Lerp(moveVertical, -4, Time.deltaTime * 3f);
        // } else {
        //     moveVertical = Mathf.Lerp (moveVertical, 0, Time.deltaTime * 5f);
        // }

        //Automatic Acceleration
        if (Input.GetAxisRaw("Vertical") < -0.5 || CrossPlatformInputManager.GetAxis("Vertical") < -0.5) //Backwards
        {
            moveVertical = Mathf.Lerp(moveVertical, 2, Time.deltaTime * 5f);
            StopDrift();
            // skidMarks.SetActive(true);
        } else if (canMove) {
            moveVertical = Mathf.SmoothStep(moveVertical, 10, Time.deltaTime * 8f);
        }
        // print(moveVertical);

        if (canBoost == true && (moveHorizontal_2 > 0.85 || Input.GetKeyDown(KeyCode.LeftShift))) //Drift Boost
        {
            driftBoost();
            // print("Drift boost");
        }if (moveHorizontal_2 < -0.85 || Input.GetKey(KeyCode.LeftControl)) {
            HandbrakeStart();
            // print("Handbrake");
        } else if (handbrake == true){
            HandbrakeStop();
        }
        if (moveVertical_2 > 0.85 || Input.GetKeyDown(KeyCode.UpArrow)) //Fire forwards
        {
            // print("Fire Forward");
            UseSkill(1);
            // Instantiate(GuidedMissilePrefab, kartModel.transform.position + kartModel.transform.forward * 5 + kartModel.transform.up, kartModel.transform.rotation, rigidBody.transform.parent);
            // Instantiate(RollingBombForwardPrefab, rigidBody.transform.position + (kartModel.transform.forward) * 2, kartModel.transform.rotation);
        } else if (moveVertical_2 < -0.85 || Input.GetKeyDown(KeyCode.DownArrow)) //Fire backwards
        {
            // print("Fire Backwards");
            UseSkill(-1);
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            startShield(10);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            startDisrupt(10);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            startInvincible();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Respawn();
        }

        if (moveHorizontal > 0) //Artificial horizontal deadzone
        {
            if (moveHorizontal < joystickDeadzone)
            {
                moveHorizontal = 0;
            } else {
                moveHorizontal = 1;
            }
        }
        if (moveHorizontal < 0)
        {
            if (moveHorizontal > -joystickDeadzone)
            {
                moveHorizontal = 0;
            } else {
                moveHorizontal = -1;
            }
        }
        if (canMove == false) //stops all movement (stun/etc)
        {
            moveHorizontal = moveVertical = moveHorizontal_2 = moveVertical_2 = 0;
        }
        if (Input.GetButtonDown("Jump") || CrossPlatformInputManager.GetButtonDown("jumpBtn"))
        {
            jumpBtn = true;
            StartCoroutine("Jump");
        }
        // print("moveVertical_2 = " + moveVertical_2);
        // print("moveHorizontal_2 = " + moveHorizontal_2);

        if (Input.GetKeyDown(KeyCode.LeftControl)) //Fire stuff here
        {
            // Instantiate(GuidedMissilePrefab, kartModel.transform.position - (kartModel.transform.forward * 3) + kartModel.transform.up, Quaternion.LookRotation(-kartModel.transform.forward), rigidBody.transform.parent);
            // Instantiate(RollingBombBackwardsPrefab, rigidBody.transform.position - (kartModel.transform.forward) * 2, kartModel.transform.rotation);
        }

        if (Input.GetButtonUp("Jump") || CrossPlatformInputManager.GetButtonUp("jumpBtn"))
        {
            // print ("JumpBtn off");
            jumpBtn = false;
            StopDrift(); //stop drift
        }

        UpdatekartStatus(); //Update kart statuses text

        transform.position = rigidBody.transform.position - new Vector3(0, 0.5f, 0); //Keeps kart model same position as rigidbody

        driftController();
        landSpeed = Mathf.RoundToInt(rigidBody.velocity.y);

        if (drifting == false) //Animation variables
        {
            animator.SetFloat(horizontalHash, moveHorizontal, 0.5f, Time.deltaTime);
            animator.SetFloat(driftDirectionHash, 0, 0.25f, Time.deltaTime); //For smoother transition
        }
        animator.SetFloat(wheelsDirectionHash, moveHorizontal, 0.25f, Time.deltaTime); //Wheel animation control
        animator.SetFloat(verticalHash, moveVertical, 0, Time.deltaTime);
        animator.speed = (playerSpeed + 1)/maxSpeed; //Sets animation speed relative to player speed

        //Skidmarks Controller
        if (onGround)
        {
            if (drifting || handbrake || Input.GetAxisRaw("Vertical") < -0.5)
            {
                skidMarks.SetActive(true);
            } else {
                skidMarks.SetActive(false);
            }
        } else {
            skidMarks.SetActive(false);
        }
    }

	void FixedUpdate () 
	{
        // rigidBody.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
        Gravity(); //Gravity 2

        //Kart Model Normal rotation (don't know what it does) (ok it rotates the kart model according to ground + kart y rotations)
		RaycastHit hitNear; 
		Physics.Raycast(transform.position + (transform.up * .1f), Vector3.down, out hitNear, 2.5f, groundLayerMask);
		kartNormal.up = Vector3.Lerp(kartNormal.up, hitNear.normal, Time.deltaTime * 8.0f);
        kartNormal.Rotate(0, transform.eulerAngles.y, 0);
        //---------//

        playerSpeed = Mathf.RoundToInt(transform.InverseTransformDirection(rigidBody.velocity).z); //Calculates player speed and int's it

        GroundDetection(); //Detects if player is on ground
        if (onGround) //Removed when moveVertical != 0 condition to make it apply always when on ground (for acceleration/decceleration)
        {
            Movement(); //Player movement
            // rigidBody.drag = initialDrag;
        } else {
            // rigidBody.drag = 0; //Removes drag in air
            AerialMovement();
            StopDrift();
            // skidMarks.SetActive(false);
            // print("Off ground");
        }
        if (drifting)
        {
            DriftingSteering();
            // trail.emitting = true; //skid marks on
        } else {
            NormalSteering();
            // trail.emitting = false; //skid marks off
        }
        if (jumpBtn && onGround && !drifting)
        {
            // StartCoroutine("Jump"); //Jump
        }
		
        LimitSpeed();
	}

    bool GroundDetection ()
    {
        Ray rearRay = new Ray(kartModel.transform.position + -kartModel.transform.forward + (kartModel.transform.up * 0.25f) , transform.TransformDirection (Vector3.down)); //changed rigidBody to kartModel due to some issues when rigidbody rotated on x axis
        Ray frontRay = new Ray(kartModel.transform.position + kartModel.transform.forward, transform.TransformDirection (Vector3.down)); //changed rigidBody to kartModel due to some issues when rigidbody rotated on x axis
        RaycastHit hit;
        // UnityEngine.Debug.DrawLine (rearRay.origin, rearRay.origin + rearRay.direction * 0.5f, Color.magenta);
        // UnityEngine.Debug.DrawLine (frontRay.origin, frontRay.origin + frontRay.direction * 0.5f, Color.blue);
        if (Physics.Raycast(rearRay, out hit, detectionDistance, groundLayerMask) || Physics.Raycast(frontRay, out hit, detectionDistance, groundLayerMask))
        {
            // print("check");
            if (onGround == false)
            {
                if (jumpBtn && drifting == false) //Drift function start
                {
                    if (moveHorizontal > 0)
                    {
                        driftRight = true;
                        StartCoroutine("Drift");
                    } else if (moveHorizontal < 0) {
                        driftLeft = true;
                        StartCoroutine("Drift");
                    }
                } else if (landSpeed < -30)
                {
                    //start nitro boost depending on land speed
                    if (landSpeed < -50)
                    {
                        nitroBonusSpeed = 20f;
                        nitroBonusTime = 4f;
                    } else if (landSpeed < -40)
                    {
                        nitroBonusSpeed = 10f;
                        nitroBonusTime = 1.5f;
                    } else {
                        nitroBonusSpeed = 2f;
                        nitroBonusTime = 1f;
                    }
                    canBoost = true;
                    startNitro(nitroBonusSpeed, nitroBonusTime);
                    // print(landSpeed);
                }
                jumpTimeEnd = Time.fixedTime;
                Airtime();
                onGround = true;
                if (hit.collider.tag == "Ground")
                {
                    onGroundTag = true;
                }
            }   
        } else 
            {
                onGround = false;
            }
        return onGround; //not useful for now
    }
    
    // Respawning function//

    public void Respawn() //Teleports player to last checkpoint position
    {
        stopDisrupt();
        stopShield();
        stopInvincible();
        StartCoroutine(PlayerStun(0, 2f));
        Vector3 lastCheckpointPos = GameController.instance.allCheckpoints[lastCheckpoint - 1].position;
        rigidBody.velocity = Vector3.zero; //Resets velocity
        transform.rotation = Quaternion.Euler(LastOngroundRotation); //Puts last checkpoint's rotation
        rigidBody.transform.position = lastCheckpointPos; //Moves player to last checkpoint
        rigidBody.transform.position += Vector3.up * 2;
        maxSpeed = defaultMaxspeed;
        forwardSpeed = defaultForwardSpeed;
        steerAngle = defaultSteerAngle;
        moveVertical = 0;
    }
    // ======= //


    IEnumerator Jump() 
    {
        if (jumping == false && onGround == true && canJump == true)
        {
            canJump = false;
            jumping = true;
            rigidBody.AddForce(kartModel.transform.TransformDirection(new Vector3(0, jumpPower, 0)), ForceMode.VelocityChange);
            detectionDistance = 0.1f;
            jumpTimeStart = Time.fixedTime;
            yield return new WaitForSeconds(jumpTime);
            // jumpTimeEnd = Time.fixedTime;
            // Airtime();
            // ResetJump();
            StartCoroutine("JumpCD");
        }
    }

    IEnumerator JumpCD ()
    {
        ResetJump();
        yield return new WaitForSeconds(0.5f);
        canJump = true;
    }

    void ResetJump () 
    {
        jumping = false;
        detectionDistance = defaultDetectionDistance;
    }

    float Airtime ()
    {
        float airtime = jumpTimeEnd - jumpTimeStart;
        // print (airtime.ToString("0.###"));
        return airtime;
    }

    IEnumerator Drift ()
    {
        //Debug.Log("Drifting!");
        // skidMarks.SetActive(true);
        drifting = true;
        canBoost = false;
        yield return new WaitForSeconds(1.0f);
        canBoost = true;
        yield return new WaitForSeconds(boostTime);
        //spin animation + playerstun here
        //spin on overdrift
        // rigidBody.maxAngularVelocity = 100;
        // rigidBody.AddTorque (new Vector3 (0, failDriftSpin * moveHorizontal, 0 ));
        canBoost = false;
        drifting = false;
    }

    void StopDrift ()
    {
        StopCoroutine ("Drift");
        drifting = false;
        driftRight = false;
        driftLeft = false;
        canBoost = false;
        driftPower = 0;
        driftStage = 0;
        // skidMarks.SetActive(false);
        //try resetting horizontal axis for smoother movement
    }

    void driftBoost()
    {
        // print(driftStage);
        startNitro(nitroBonusSpeed * (driftStage + 1), nitroBonusTime);
        if(driftStage == 2)
        {
            StopDrift();
            return;
        }
        driftPower = 0;
        driftStage += 1;
        //try resetting horizontal axis for steering after smoother boost
        StopCoroutine("Drift");
        StartCoroutine("Drift");
    }

    void driftController() //controls drifting animations and drifting interruptions
    {
        if (driftPower > 100f)
        {
            StopDrift();
            // do a spin
        }
        if (driftPower > 90)
        {
            nitroBonusSpeed = 10f;
            nitroBonusTime = 3f;
            driftBar.color = Color.red;
            // exhaustParticles.startColor = new Color(26,26,26);
            //start drift3 animation/particles
        } else if (driftPower > 60)
        {
            nitroBonusSpeed = 5f;
            nitroBonusTime = 1.5f;
            driftBar.color = Color.yellow;
            //start drift2 animation/particles
        } else if (driftPower < 60)
        {
            nitroBonusSpeed = 3f;
            nitroBonusTime = 1f;
            driftBar.color = Color.green;
            //start drift1 animation/particles
        }
        driftBar.fillAmount = driftPower / 100;
    }

    void startNitro(float bonusSpeed, float nitroTime)
    {
        if (canBoost)
        {
            if (nitro && !nitroPadBoost) //Already boosted from nitro (not nitropad)
            {
                StopCoroutine(nitroCoroutine); //Stops boost
                forwardSpeed -= prevNitroBonusSpeed; //Reverts previous boosts
                maxSpeed -= prevNitroBonusSpeed/2; //Added this to remove last bonus speed (it changes)
            }
            nitroCoroutine = StartCoroutine(Nitro(bonusSpeed, nitroTime)); //Restarts coro
            prevNitroBonusSpeed = bonusSpeed;
        }
    }

    IEnumerator Nitro (float bonusForwardSpeed, float bonusTime)
    {
        //Debug.Log("Start Nitro");
        canBoost = false;
        nitro = true;
        // rigidBody.AddForce(kartModel.transform.forward * driftPower, ForceMode.Impulse); //Adds boost depending on drift power value
        maxSpeed += bonusForwardSpeed/2;
        forwardSpeed += bonusForwardSpeed;
        yield return new WaitForSeconds(bonusTime);
        maxSpeed -= bonusForwardSpeed/2;
        forwardSpeed -= bonusForwardSpeed;
        nitro = false;
        //Debug.Log("Stop Nitro");
    }

    public void startNitroPadBoost()
    {
        if (nitroPadBoost)
        {
            StopCoroutine(nitropadBoostCoroutine);
            // forwardSpeed -= 10f;
            maxSpeed -= 5f;
            steerAngle = defaultSteerAngle;
        }
        nitropadBoostCoroutine = StartCoroutine(NitroPadBoost());
    }

    IEnumerator NitroPadBoost () 
    {
            nitroPadBoost = true;
            nitro = true;
            // rigidBody.AddForce(kartModel.transform.forward * 50, ForceMode.Impulse); //Adds fixed boost (not visible to due fixed camera pos)
            maxSpeed += nitroBonusTime;
            forwardSpeed += 10f;
            maxSpeed += 5f;
            yield return new WaitForSeconds(5.0f);
            forwardSpeed -= 10f;
            maxSpeed -= 5f;
            nitro = false;
            nitroPadBoost = false;
    }

    void StopAllNitro()
    {
        if (nitroCoroutine != null)
        {
            StopCoroutine(nitroCoroutine); //stops all nitro instances
        }
        if (nitropadBoostCoroutine != null)
        {
            StopCoroutine(nitropadBoostCoroutine);
        }
        nitroPadBoost = false;
        maxSpeed = defaultMaxspeed;
        forwardSpeed = defaultForwardSpeed;
        steerAngle = defaultSteerAngle;
        nitro = false;
    }

    void Movement ()
    {
        if (canMove)
        {
            if (moveVertical > 0)
            {
                rigidBody.AddForce(kartNormal.transform.forward * forwardSpeed * moveVertical, ForceMode.Acceleration); //changed the forward so animation can rotate kart freely without changing forward movement direction
            }
        }
    }

    void AerialMovement() //control in the air
    {
        rigidBody.AddForce(kartNormal.transform.forward * moveVertical * forwardSpeed * 0.15f, ForceMode.Acceleration);
    }
    void NormalSteering()
    {
        if (playerSpeed >= 0)
        {
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y + moveHorizontal * steerAngle, 0), Time.deltaTime * 3f);
        } else if (playerSpeed < 0) {
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y - moveHorizontal * steerAngle, 0), Time.deltaTime * 3f);
        }
        //Debug.Log("Normal Movement");
    }

    void DriftingSteering()
    {
        if (driftRight)
        {
            if (moveHorizontal < 0)
            {
                //Half drift right (forward)
                transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y + (moveHorizontal + 1) * driftingSteerAngle, 0), Time.deltaTime * 3f);
            } else {
                //Fully drift right
                transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y + (moveHorizontal * 0.5f + 1) * driftingSteerAngle, 0), Time.deltaTime * 3f);
            }
            animator.SetFloat(driftDirectionHash, moveHorizontal + 1f, 0.25f, Time.deltaTime);
            // print ("right drift");
        } else if (driftLeft)
        {
            if (moveHorizontal > 0)
            {
                //Half drift left (forward)
                transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y + (moveHorizontal - 1) * driftingSteerAngle, 0), Time.deltaTime * 3f);
            } else {
                //Fully drift left
                transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y + (moveHorizontal * 0.5f - 1) * driftingSteerAngle, 0), Time.deltaTime * 3f);
            }
            animator.SetFloat(driftDirectionHash, moveHorizontal - 1f, 0.25f, Time.deltaTime);
            // print ("left drift");
        }
        rigidBody.transform.rotation = Quaternion.Lerp(rigidBody.rotation, playerRotation, 1f); //idk what this does
        // driftPower = Mathf.Lerp(driftPower, 100f, 0.005f);
        driftPower += 50f * Time.deltaTime;
//        Debug.Log("Drifting Movement");
    }

    void HandbrakeStart()
    {
        if (onGround)
        {
            StopAllNitro();
            StopDrift();
            moveVertical = 1f; //Car still moves but very slowly
            handbrake = true;
            rigidBody.drag = initialDrag / 2f;
            // forwardSpeed = defaultForwardSpeed / 8f;
            steerAngle = defaultSteerAngle * 2f;
            //Add skid mark start
            // skidMarks.SetActive(true);
        }
    }

    void HandbrakeStop()
    {
        handbrake = false;
        forwardSpeed = defaultForwardSpeed;
        rigidBody.drag = initialDrag;
        steerAngle = defaultSteerAngle;
        //Stop skid marks
        // skidMarks.SetActive(false);
    }

    void startInvincible()
    {
        if (createdInvincible != null) //Checks if player already invincible
        {
            StopCoroutine(invincibleCoroutine); //Stops current invincible state
        }
        stopDisrupt();
        StopAllNitro();
        invincibleCoroutine = StartCoroutine(Invincible()); //Restarts it
    }

    IEnumerator Invincible()
    {
        canMove = true;
        canBeStunned = false;
        maxSpeed += 5f;
        forwardSpeed += 10f;
        if(createdInvincible == null) //No effect
        {
            createdInvincible = Instantiate(InvinciblePrefab, kartNormal);
        }
        yield return new WaitForSeconds(10f);
        stopInvincible(); 
    }

    void startShield(float time)
    {
        if (createdShield != null) //Already exists shield
        {
            StopCoroutine(shieldCoroutine); //stop shield
        }
        stopDisrupt(); //Remove debuffs here
        shieldCoroutine = StartCoroutine(Shield(time)); //Restart
    }

    IEnumerator Shield(float time)
    {
        if (createdShield == null) //if there's no created shield effect
        {
            createdShield = Instantiate(ShieldPrefab, kartModel); //adds one
        }
        yield return new WaitForSeconds(time);
        stopShield();
    }

    public void startDisrupt(float time)
    {
        if (createdShield == null && canBeStunned) //not shielded/invincible
        {
            if (createdDisruptedEffect != null) //If player is already disrupted
            {
                StopCoroutine(disruptedCoroutine);
                // print("restarted disrupt");
            }
            StopDrift();
            StopAllNitro();
            disruptedCoroutine = StartCoroutine(disrupt(time)); //Restarts coro
            // print("started disrupt");
        } else {
            stopShield();
            // print("removed shield");
        }
    }
    public IEnumerator disrupt(float time)
    {
        forwardSpeed = defaultForwardSpeed * 0.6f;
        steerAngle = defaultSteerAngle * 0.75f;
        if (createdDisruptedEffect == null)
        {
            createdDisruptedEffect = Instantiate(slowedEffectPrefab, kartNormal);
        }
        yield return new WaitForSeconds(time);
        stopDisrupt();
    }

    void LimitSpeed ()
    {
        // if (rigidBody.velocity.sqrMagnitude > maxSpeed * maxSpeed && onGround == true)
        // {
        //     rigidBody.velocity = rigidBody.velocity.normalized * maxSpeed;
        // }
        if (playerSpeed > maxSpeed && onGround == true)
        {
            rigidBody.velocity = rigidBody.velocity.normalized * maxSpeed;
        }
    }

    void Gravity () 
    {
        rigidBody.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }

    public void addPowerOrb(int count)
    {
        StartCoroutine(addPowerOrbCo(count));
    }

    private IEnumerator addPowerOrbCo(int addCount)
    {
        if (powerOrbCount < 10)
        {
            resetMaxSpeed();
            for (int i = 0; i < addCount; i++)
            {
                powerOrbCount += 1;
                UpdatePowerOrbCounter();
                yield return new WaitForSeconds(0.25f);
                if (powerOrbCount == 10)
                {
                    increaseMaxSpeed(); //Change this to increase maxSpeed
                    yield break;
                }
            }
        }
    }

    private void RemovePowerOrb(int count)
    {
        if (powerOrbCount < count)
        {
            powerOrbCount = 0;
        } else {
            powerOrbCount -= count;
        }
        resetMaxSpeed();
        UpdatePowerOrbCounter();
    }

    void increaseMaxSpeed() //Change to increase max speed
    {
        defaultMaxspeed += 5;
        maxSpeed = defaultMaxspeed;
        //Adds boosted effect
        if (createdPoweredUp == null)
        {
            createdPoweredUp = Instantiate(poweredUpPrefab, kartNormal);
        }
    }

    void resetMaxSpeed() //Change to lower to default maxSpeed
    {
        if (powerOrbCount == 10)
        {
            defaultMaxspeed -= 5;
            maxSpeed = defaultMaxspeed;
        }
        //Removes boosted effect
        Destroy(createdPoweredUp);
    }

    public IEnumerator getSkill ()
    {
        int[] currentSkillset;
        if (racePosition == 1)
        {
            //Use winning skill set to get skills
            currentSkillset = WinningSkillSet;
        } else {
            //Use losing skillset
            currentSkillset = LosingSkillSet;
        }
        canUseSkill = false;
        canPowerup = false;
        skillNumber = UnityEngine.Random.Range(0, currentSkillset.Length); //spicifies which powerupsSprites to cycle from
        int prevNumber = -1;
        float powerupTimer = 0;
        powerupImage.enabled = true;
        while (powerupTimer < 4)
        {
            while (skillNumber == prevNumber)
            {
                skillNumber = UnityEngine.Random.Range(0, currentSkillset.Length); //prevents getting duplicates
            }

            powerupImage.sprite = powerupsSprites[currentSkillset[skillNumber]]; //cycles through powerupsSprites (because array index start from zero)
            
            prevNumber = skillNumber;

            // print(currentSkillset[skillNumber]);

            yield return new WaitForSeconds(0.2f);
            powerupTimer += 0.25f;
        }
        canPowerup = true;
        //returns final skill number here
        // skillNumber = 0; //(set custom skill) {"0 - Guided Missile", "1 - Nitro", "2 - Death Skull", "3 - Global Disrupt", "4 - TnT", "5 - Poison Bottle", "6 - Rolling Grenade", "7 - Sheild", "8 - Oobaga", "9 - Fruits"}
        powerupImage.sprite = powerupsSprites[currentSkillset[skillNumber]];
        skillNumber = currentSkillset[skillNumber]; //Sets the skillNumber to actual number in skillset
        canUseSkill = true;
        // print(skillNumber); //skill number
        // print(skills[skillNumber]); //name of skill
    }

    public void startStun(float timeBeforeStun2, float stunTime2)
    {
        StartCoroutine(PlayerStun(timeBeforeStun2, stunTime2));
    }
    
    IEnumerator PlayerStun(float timeBeforeStun, float stunTime)
    {
        if (canBeStunned == true && createdShield == null) //no shield active
        {
            yield return new WaitForSeconds(timeBeforeStun);
            RemovePowerOrb(5);
            resetMaxSpeed();
            canMove = false;
            StopDrift();
            StopAllNitro();
            yield return new WaitForSeconds(stunTime);
            canMove = true;
        } else {
            stopShield();
        }
    }

    public void AddCheckpoint(int checkpointNum)
    {
        if (checkpointNum >= nextCheckpoint && checkpointNum < nextCheckpoint + 5) //checks if it's the next checkpoint or more (if player skips some -max skip 5-)
        {
            if (checkpointNum == 1)
            {
                Lap();
            } else {
                nextCheckpoint = checkpointNum + 1;
            }
            //Saves rotation
            // LastOngroundRotation = new Vector3 (0, Mathf.Round(kartNormal.transform.rotation.eulerAngles.y/90) * 90, 0); //Rounds rotation to nearest 90 degrees
            lastCheckpoint = checkpointNum; //Used in getting race position and respawn
            LastOngroundRotation = new Vector3 (0, GameController.instance.allCheckpoints[lastCheckpoint - 1].rotation.eulerAngles.y, 0); //Gets rotation of last checkpoint
        } else {
            //Add wrong way logic
        }
        if (nextCheckpoint == numberOfCheckpoints + 1) //No more checkpoints
        {
            nextCheckpoint = 1;
        }
        nextCheckpointPos = GameController.instance.allCheckpoints[nextCheckpoint - 1].position; //Gets pos from checkpoints pos array
    }

    void Lap()
    {
        if (LapNumber == numberOfLaps) //Reached max laps
        {
            lapTimes[LapNumber - 1] = lapTimer.text;
            matchEnd();
        } else {
            if (LapNumber != 0) //If not at beginning of match (when lapnumber = 0)
            {
                // lapTimes[LapNumber - 1] = lapTimer.text; //can't index -1 lapnumber
                //Replaced with this to update once a lap instead of every frame
                timeElapsed = (int)(Time.time - lapStartTime);
                lapTimes[LapNumber - 1] = timeElapsed.ToString() + " seconds";
            }
            LapNumber += 1;
            nextCheckpoint = 2; //next checkpoint
            UpdateLapText();
            restartTimer();
        }
    }

    void UpdateLapText()
    {
        lapCount.text = "LAP: " + LapNumber + "/" + numberOfLaps;
    }

    public void UpdatePosition() //Creates custom int to compare player distances with each other
    {
        distanceToCheckpoint();
        //Max LapNumber = 99, max checkpoints = 99, max distancefromcheckpoint = 999
        playerPositions = LapNumber * 100000 + lastCheckpoint * 1000 - distanceFromNextChkPnt/100;
        GameController.instance.kartPositions(kartIndex, playerPositions, 0);
        racePosText.text = racePosition + "/" + numberOfPlayers;
    }

    void distanceToCheckpoint() //Calculates distance between player and nextcheckpoint, nextCheckpointPos changed onTrigger
    {
        // distanceFromNextChkPnt = Vector3.Distance(rigidBody.position, nextCheckpointPos);
        distanceFromNextChkPnt = Vector2.SqrMagnitude(rigidBody.position - nextCheckpointPos);
    }

    void matchEnd()
    {
        GameController.instance.endMatch(); //removes other canvases
        canMove = false;
        lapTimesFinal.text = "Lap Times:";
        for (int i = 0; i < lapTimes.Length; i++)
        {
            lapTimesFinal.text += "\n" + lapTimes[i];
        }
        if (racePosition == 1)
        {
            finalRankText.text = racePosition + "ST PLACE";
        } else if (racePosition == 2)
        {
            finalRankText.text = racePosition + "ND PLACE";
        } else if (racePosition == 3)
        {
            finalRankText.text = racePosition + "RD PLACE";
        } else {
            finalRankText.text = racePosition + "TH PLACE";
        }
        //Send maptimes to timeTrial if this mode is enabled
        //switch to AI movement (add later after I find out how to make AI movement :'D)
        //change to endmatch canvas and stop what functions you need to
    }

    //All SKILLS:
    //Skill numbers:
    // "0-Nitro" -self skill-
    // "1-Fruits" -self skill-
    // "2-Guided Missile"
    // "3-Death Skull"
    // "4-Global Disrupt"
    // "5-TnT"
    // "6-Poison Bottle"
    // "7-Rolling Grenade"
    // "8-Shield" 
    // "9-Invincibility" 
    //Skills at the beginning do not require prefabs/skill pooler (nitro, fruits)
    //To get a skill prefab from skillPooler we subtract no of selfSkills with skillnumber
    //Ex: skill number = 5 (TnT), selfSkills = 2; skillPooler.getSkill(5-2) = 3rd item in pooler (TnT)
    // selfSkills = 2; //Number of self skills
    int skillOffset = 0;
    void UseSkill(float direction)
    {
        if (canUseSkill)
        {
            if (direction >= 0)
            {
                //Forward
                skillOffset = 5;
            } else {
                //Backward
                skillOffset = -3;
            }
            if (skillNumber == 0)
            {
                //Nitro
                startNitro(10f, 4f);
            } else if (skillNumber == 1)
            {
                //Add power
                addPowerOrb(5);
            } else {
                Skills.instance.getSkill(skillNumber, kartModel.transform.position + kartModel.transform.forward * skillOffset + kartModel.transform.up * 2, kartModel.rotation.eulerAngles + Vector3.up * (90 * (-1 + direction)), transform.parent); //Flips skill y-rotation by 180 when fired backwards
            }
            canUseSkill = false;
            powerupImage.enabled = false;
        }
    }
    //Player skill scripts here \/ \/ \/


    void UpdatekartStatus ()
    {
        if (canBoost)
        {
            kartStatus.text = "Can Boost!";
        }
        else if (nitro)
        {
            kartStatus.text = "Nitro";
        }
        else if (drifting)
        {
            kartStatus.text = "Drifting!";
        }
        else if (onGround)
        {
            kartStatus.text = "On ground!";
        } 
        else if (!onGround)
        {
            kartStatus.text = "Off ground!";
        }
        else
        {
            kartStatus.text = "";
        }
        // driftPowerText.text = Mathf.RoundToInt(driftPower).ToString(); //Disabled for too much GC -find replacement for ToString() or call less times
    }

    void UpdatePlayerSpeed () 
    {
        speedoMeter.text = "Speed: " + playerSpeed.ToString(); //Disabled for too much GC -find replacement for ToString() or call less times
    }

    void UpdatePowerOrbCounter()
    {
        powerOrbCounter.text = "Power: " + powerOrbCount.ToString() + "/10"; //Disabled for too much GC -find replacement for ToString() or call less times
    }

    public void changeMaxSpeed(float newMaxspeed)
    {
        maxSpeed = newMaxspeed;
        forwardSpeed = maxSpeed * 1.5f;
    }

    public void changeJoystickDeadzone(float newValue)
    {
        joystickDeadzone = newValue / 10f;
    }

    void stopShield()
    {
        if (createdShield != null)
        {
            Destroy(createdShield);
        }
        if (shieldCoroutine != null)
        {
            StopCoroutine(shieldCoroutine);
        }
    }

    void stopInvincible()
    {
        if (createdInvincible != null)
        {
            Destroy(createdInvincible);
        }
        if (invincibleCoroutine != null)
        {
            StopCoroutine(invincibleCoroutine);
        }
        canBeStunned = true;
        maxSpeed -= -5;
        forwardSpeed -= 10f;
    }

    void stopDisrupt()
    {
        if (createdDisruptedEffect != null)
        {
            Destroy(createdDisruptedEffect);
        }
        if (disruptedCoroutine != null)
        {
            StopCoroutine(disruptedCoroutine);
            forwardSpeed = defaultForwardSpeed;
            steerAngle = defaultSteerAngle;
        }
    }

    void stopPoweredUp()
    {
        if (createdPoweredUp != null)
        {
            Destroy(createdPoweredUp);
        }
    }

    void updateKartSound()
    {
        //Normal engine has lower pitch limit
        //Pitch limit increased while drifting/jumping
        //Pitch limit increased even more while nitroed
        // audioSrc.pitch = 0.75f + playerSpeed/defaultMaxspeed * 2f;
        float basePitch = 1f;
        audioSrc.pitch = Mathf.Lerp(audioSrc.pitch, basePitch + playerSpeed/defaultMaxspeed, Time.deltaTime * 5);
        if (nitro)
        {
            audioSrc.pitch = Mathf.Lerp(audioSrc.pitch, basePitch * 1.5f + playerSpeed/defaultMaxspeed * 1.5f, Time.deltaTime * 20);
        } else if (drifting || jumping)
        {
            audioSrc.pitch = Mathf.Lerp(audioSrc.pitch, basePitch * 1.5f + playerSpeed/defaultMaxspeed, Time.deltaTime * 20);
        }
    }

    public void startTimer()
    {
        lapStartTime = Time.time;
    }

    void restartTimer()
    {
        lapStartTime = Time.time;
    }

    void OnDestroy()
    {
        if (GameController.instance != null)
        {
            try {
            GameController.instance.StopCoroutine(GameController.instance.raceStartCorou); //stops raceStart coroutine, prevents error when user exists race on start   
            } catch {
                //Nothing
            }
        }
    }
}