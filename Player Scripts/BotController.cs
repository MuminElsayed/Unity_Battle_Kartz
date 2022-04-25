using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.Events;
using System.Diagnostics;
using TMPro;

public class BotController : MonoBehaviour {

    [Header("Kart Stats")]
    [SerializeField]
    private float forwardSpeed;
    [SerializeField]
    private float steerAngle, driftingSteerAngle, detectionDistance = 1.5f, maxSpeed, jumpPower = 10, jumpTime, gravity, initialDrag;
    //----------------//
    private int playerSpeed;
    private float defaultForwardSpeed;
    private float defaultDetectionDistance;
    private float defaultMaxspeed;
    private float defaultSteerAngle;
    [Header("Skill Set")]
    [SerializeField]
    private int[] skillset; //Predifined skills by array order in "PooledSkills"

    [Header("Kart Statuses")]
    
    [SerializeField]
    private int landSpeed, nextCheckpoint = 1;
    public int racePosition, numberOfLaps, kartIndex;
    private Vector3 nextCheckpointPos;
    public bool canMove = true;
    [SerializeField]
    private bool onGround = false;

    //----------------//

    private bool drifting = false;
    [SerializeField]
    private float moveHorizontal, distanceFromNextChkPnt, playerPositions;
    public float highestDistance;
    private Vector3 targetRelativeRotation;
    private bool canBoost = false;
    private bool nitro;
    private bool nitroPadBoost = false;
    private bool canUseSkill = false;
    private bool canBeStunned = true;
    private bool shielded = false;
    private bool handbrake;
    private int LapNumber = 0;
    private int lastCheckpoint = 1;
    private int numberOfCheckpoints;

    [Header("Kart References")]
    
    public bool canPowerup = true;
	[SerializeField]
    private Transform kartNormal, kartModel;
    [SerializeField]
    private Rigidbody rigidBody;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private ParticleSystem[] ps;
    private int difficulty;
    [SerializeField]
    private Material normalMat, nitroMat, canBoostMat;
    [SerializeField]
    private GameObject skidMarks;
    [SerializeField]
    private bool canDrift = true;
    
    //----------------//
    private LayerMask groundLayerMask;
    private Vector3 LastOngroundRotation;
    [SerializeField]
    private float moveVertical;
    public int skillNumber;

    [Header("Others")]
    private Renderer psRenderer1;
    private Renderer psRenderer2;
    private int horizontalHash = Animator.StringToHash("Horizontal");
    private int verticalHash = Animator.StringToHash("Vertical");
    private int wheelsDirectionHash = Animator.StringToHash("WheelsDirection");
    private int driftDirectionHash = Animator.StringToHash("DriftDirection");


    void Start () 
    {
        rigidBody = rigidBody.GetComponent<Rigidbody>();
        initialDrag = rigidBody.drag;
        difficulty = GameController.instance.difficulty;
        defaultForwardSpeed = forwardSpeed + ((5 * difficulty) - 10); //Changes depending on difficulty (-10 to +15)
        forwardSpeed = defaultForwardSpeed;
        defaultMaxspeed = maxSpeed + ((difficulty * 5) - 10); //Changes depending on difficulty (-10 to +15)
        maxSpeed = defaultMaxspeed;
        defaultSteerAngle = steerAngle + ((difficulty * 5) - 10); //Changes depending on difficulty (-10 to +15)
        steerAngle = defaultSteerAngle;
        driftingSteerAngle = driftingSteerAngle + ((difficulty * 5) - 10) * 2; //Changes depending on difficulty (-20 to +30)
        defaultDetectionDistance = detectionDistance;
		groundLayerMask = LayerMask.GetMask("Ground");
        ps = GetComponentsInChildren<ParticleSystem>();
        numberOfCheckpoints = GameController.instance.checkpoints.transform.childCount;
        nextCheckpointPos = GameController.instance.allCheckpoints[0].position + GameController.instance.allCheckpoints[nextCheckpoint - 1].right * UnityEngine.Random.Range(-10f, 10f); //Gets first checkpoint pos
        psRenderer1 = ps[0].GetComponent<Renderer>();
        psRenderer2 = ps[1].GetComponent<Renderer>();
        InvokeRepeating("startDrift", 10f + (kartIndex * 2), 5 * (6 - difficulty)); //Start time: 12-24 secs, repeat rate: 25, 20, 15, 10, 5 secs (depending on difficulty)
    }

    void Update () 
    {
        if (canBoost == true)
        {
            psRenderer1.material = canBoostMat;
            psRenderer2.material = canBoostMat;
        } else if (nitro == true){
            psRenderer1.material = nitroMat;
            psRenderer2.material = nitroMat;
        } else {
            psRenderer1.material = normalMat;
            psRenderer2.material = normalMat;
        }

        //Bot movement
        Vector3 targetPos = new Vector3 (nextCheckpointPos.x, transform.position.y, nextCheckpointPos.z); //Removed random number on x/z axis
        Quaternion targetRot = Quaternion.LookRotation(targetPos - transform.position); 
        

        if (canMove) //New Method
        {
            moveVertical = Mathf.SmoothStep(moveVertical, 10, Time.deltaTime * 8f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, Time.deltaTime * steerAngle);
            animator.SetFloat(wheelsDirectionHash, moveHorizontal, 0.25f, Time.deltaTime); //Wheel animation control
            animator.SetFloat(horizontalHash, moveHorizontal, 0.25f, Time.deltaTime); //Kart animation
            targetRelativeRotation = transform.InverseTransformPoint(targetPos);
            if (drifting) //Drifting animation
            {
                if (moveHorizontal != 0) //If already steering
                {
                    animator.SetFloat(driftDirectionHash, moveHorizontal, 0.25f, Time.deltaTime);
                } else {
                    animator.SetFloat(driftDirectionHash, -1, 0.25f, Time.deltaTime); //If not, sets random drift direction
                }
            } else {
                animator.SetFloat(driftDirectionHash, 0, 0.5f, Time.deltaTime);
            }
            if (targetRelativeRotation.x > 3f) //Target checkpoint to the right -- moving right
            {
                if (targetRelativeRotation.x > 25f)
                {
                    HandbrakeStart();
                    // print("Handbrake Right");
                } else if (targetRelativeRotation.x > 10f && drifting == false)
                {
                    HandbrakeStart();
                    // print(targetRelativeRotation.x);
                } else if (handbrake) {
                    HandbrakeStop();
                }
                moveHorizontal = 1;
            } else if (targetRelativeRotation.x < -3f) //Target checkpoint to the left -- moving left
            {
                if (targetRelativeRotation.x < -25f)
                {
                    HandbrakeStart();
                    // print("Handbrake Left");
                } else if (targetRelativeRotation.x < -10f && drifting == false)
                {
                    HandbrakeStart();
                    // print(targetRelativeRotation.x);
                } else if (handbrake){
                    HandbrakeStop();
                }
                moveHorizontal = -1;
            } else { //Target checkpoint forward //Moving forward
                moveHorizontal = 0;
            }
        } else {
            moveVertical = 0;
            moveHorizontal = 0;
        }

        animator.SetFloat(verticalHash, moveVertical, 0.25f, Time.deltaTime);
        animator.speed = (playerSpeed + 1)/maxSpeed;

        UnityEngine.Debug.DrawLine(transform.position, targetPos, Color.red); //shows where targetpos is

        transform.position = rigidBody.transform.position - new Vector3(0, 0.5f, 0); //Keeps kart model same position as rigidbody

        landSpeed = Mathf.RoundToInt(rigidBody.velocity.y);

        //Skidmarks effect
        if (onGround)
        {
            if (drifting || handbrake)
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
        Gravity(); //Gravity 2

        //Kart Model Normal rotation (don't know what it does) (ok it rotates the kart model according to ground + kart y rotations)
		RaycastHit hitNear; 
		Physics.Raycast(transform.position + (transform.up * .1f), Vector3.down, out hitNear, 2.5f, groundLayerMask);
		kartNormal.up = Vector3.Lerp(kartNormal.up, hitNear.normal, Time.deltaTime * 8.0f);
        kartNormal.Rotate(0, transform.eulerAngles.y, 0);
        //---------//

        playerSpeed = Mathf.RoundToInt(transform.InverseTransformDirection(rigidBody.velocity).z); //Calculates player speed and int's it

        GroundDetection(); //Detects if player is on ground

        if (onGround == true) //Removed when moveVertical != 0 condition to make it apply always when on ground (for acceleration/decceleration)
        {
            Movement(); //Player movement
        } else {
            AerialMovement();
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
                if (landSpeed < -30)
                {
                    //start nitro boost on landing
                    canBoost = true;
                    StartCoroutine("Nitro");
                }
                onGround = true;
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
        StartCoroutine(PlayerStun(0, 2f));
        Vector3 lastCheckpointPos = GameController.instance.allCheckpoints[lastCheckpoint - 1].position;
        rigidBody.velocity = Vector3.zero; //Resets velocity
        transform.rotation = Quaternion.Euler(LastOngroundRotation); //Puts last checkpoint's rotation
        rigidBody.transform.position = lastCheckpointPos; //Moves bot to last checkpoint
        rigidBody.transform.position += Vector3.up * 2;
        maxSpeed = defaultMaxspeed;
        forwardSpeed = defaultForwardSpeed;
        steerAngle = defaultSteerAngle;
    }

    IEnumerator Jump()
    {
        if (onGround)
        {
            rigidBody.AddForce(kartModel.transform.TransformDirection(new Vector3(0, jumpPower, 0)), ForceMode.VelocityChange);
            detectionDistance = 0.1f;
            yield return new WaitForSeconds(jumpTime);
            // jumpTimeEnd = Time.fixedTime;
            // Airtime();
            ResetJump();
        }
    }

    void ResetJump () 
    {
        detectionDistance = defaultDetectionDistance;
    }



    void StopDrift ()
    {
        StopCoroutine ("drift");
        drifting = false;
        canBoost = false;
        //try resetting horizontal axis for smoother movement
    }


    IEnumerator Invincible()
    {
        canBeStunned = false;
        maxSpeed += 5f;
        forwardSpeed += 10f;
        //add can't be stunned logic
        yield return new WaitForSeconds(5f);
        canBeStunned = true;
        maxSpeed -= 5f;
        forwardSpeed -= 10f;
    }

    IEnumerator shield(float time)
    {
        shielded = true;
        yield return new WaitForSeconds(time);
        shielded = false;
    }

    IEnumerator Nitro ()
    {
        if (canBoost == true) 
        {
            canBoost = false;
            //Debug.Log("Start Nitro");
            nitro = true;
            maxSpeed = defaultMaxspeed + difficulty * 2;
            forwardSpeed = defaultForwardSpeed + difficulty * 4;
            yield return new WaitForSeconds(difficulty);
            forwardSpeed = defaultForwardSpeed;
            nitro = false;
            maxSpeed = defaultMaxspeed;
            //Debug.Log("Stop Nitro");
        }
    }

    public void startNitroPadBoost()
    {
        StartCoroutine(NitroPadBoost());
    }

    IEnumerator NitroPadBoost () 
    {
        if (nitroPadBoost == false)
        {
            nitroPadBoost = true;
            nitro = true;
            // maxSpeed += nitroBonusTime;
            forwardSpeed = defaultForwardSpeed * 1.2f;
            maxSpeed = defaultMaxspeed * 1.1f;
            //for smoother steering//
            // steerAngle /= 2;
            // yield return new WaitForSeconds(0.2f);
            // steerAngle = defaultSteerAngle;
            //----//
            yield return new WaitForSeconds(4.0f);
            forwardSpeed = defaultForwardSpeed;
            maxSpeed = defaultMaxspeed;
            nitro = false;
            // maxSpeed -= nitroBonusTime;
            nitroPadBoost = false;
        }
    }

    void StopNitro()
    {
        StopCoroutine("Nitro"); //stops all nitro instances
        StopCoroutine("NitroPadBoost");
        maxSpeed = defaultMaxspeed;
        forwardSpeed = defaultForwardSpeed;
        nitro = false;
    }

    void Movement ()
    {
        if (canMove)
        {
            rigidBody.AddForce(kartNormal.transform.forward * forwardSpeed * moveVertical, ForceMode.Acceleration); //changed the forward to kartNormal so animation can rotate kart freely without changing forward movement direction
        }
    }

    void AerialMovement() //control in the air
    {
        rigidBody.AddForce(kartNormal.transform.forward * moveVertical * forwardSpeed * 0.15f, ForceMode.Acceleration);
    }


    //Might use on tight turns logic
    void HandbrakeStart()
    {
        StopNitro();
        StopDrift();
        moveVertical = 1f; //Car still moves but very slowly
        handbrake = true;
        rigidBody.drag = initialDrag / 2f;
        // forwardSpeed = defaultForwardSpeed / 8f;
        steerAngle = defaultSteerAngle * 2f;
    }

    void HandbrakeStop()
    {
        handbrake = false;
        forwardSpeed = defaultForwardSpeed;
        rigidBody.drag = initialDrag;
        steerAngle = defaultSteerAngle;
    }

    public void startDisrupt(float time)
    {
        StartCoroutine(disrupt(time));
    }
    public IEnumerator disrupt(float time)
    {
        // print("a7a");
        forwardSpeed = defaultForwardSpeed * 0.75f;
        steerAngle = defaultSteerAngle * 0.5f;
        StopDrift();
        StopNitro();
        yield return new WaitForSeconds(time);
        forwardSpeed = defaultForwardSpeed;
        steerAngle = defaultSteerAngle;
    }

    void LimitSpeed ()
    {
        if (playerSpeed > maxSpeed && onGround == true)
        {
            rigidBody.velocity = rigidBody.velocity.normalized * maxSpeed;
        }
    }

    void Gravity () 
    {
       rigidBody.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }

    public void startGetSkill()
    {
        StartCoroutine(getSkill());
    }

    IEnumerator getSkill ()
    {
        // print("Got a skill");
        canUseSkill = false;
        canPowerup = false;
        skillNumber = UnityEngine.Random.Range(0, skillset.Length); //spicifies which powerups to cycle from
        yield return new WaitForSeconds(2f); //wait before firing
        canUseSkill = true;
        skillNumber = skillset[skillNumber]; //Sets actual skill from set
        UseSkill();
        canPowerup = true;
    }

    public void startStun(float timeBeforeStun2, float stunTime2)
    {
        StartCoroutine(PlayerStun(timeBeforeStun2, stunTime2));
    }
    
    IEnumerator PlayerStun(float timeBeforeStun, float stunTime)
    {
        if (canBeStunned == true && shielded == false)
        {
            yield return new WaitForSeconds(timeBeforeStun);
            StopDrift();
            StopNitro();
            canMove = false;
            yield return new WaitForSeconds(stunTime);
            canMove = true;
        } else {
            shielded = false;
        }
    }

    public void AddCheckpoint(int checkpointNum)
    {
        if (checkpointNum == nextCheckpoint) //checks if it's the next checkpoint
        {
            if (checkpointNum == 1) //If it's the first checkpoint (start of lap)
            {
                Lap();
            } else {
                nextCheckpoint += 1;
            }
            //Saves rotation
            // LastOngroundRotation = new Vector3 (0, Mathf.Round(kartNormal.transform.rotation.eulerAngles.y/90) * 90, 0); //Rounds rotation to nearest 90 degrees
            lastCheckpoint = checkpointNum; //Used in getting race position and respawn
            LastOngroundRotation = new Vector3 (0, GameController.instance.allCheckpoints[lastCheckpoint - 1].rotation.eulerAngles.y, 0);
        }
        if (nextCheckpoint == numberOfCheckpoints + 1) //No more checkpoints
        {
            nextCheckpoint = 1;
        }
        nextCheckpointPos = GameController.instance.allCheckpoints[nextCheckpoint - 1].position + GameController.instance.allCheckpoints[nextCheckpoint - 1].right * UnityEngine.Random.Range(-10f/(difficulty + 1), 10f/(difficulty + 1));; //Gets pos from checkpoints pos array
    }

    void Lap()
    {
        if (LapNumber == numberOfLaps) //Reached max laps
        {
            matchEnd();
            LapNumber += 1;
        } else {
            if (LapNumber != 0) //If not at beginning of match (when lapnumber = 0)
            {
                LapNumber += 1;
                nextCheckpoint = 2; //next checkpoint
                // stopwatch.Reset();
            } else {
                LapNumber += 1;
                nextCheckpoint = 2; //next checkpoint
                // stopwatch.Reset();
            }
        }
    }


    public void UpdatePosition() //Creates custom string to compare player distances with each other
    {
        distanceToCheckpoint();
        //Max LapNumber = 99, max checkpoints = 99, max distancefromcheckpoint = 999
        // string distance = LapNumber.ToString("00") + nextCheckpoint.ToString("000") + distanceFromNextChkPnt.ToString("0000.0");
        playerPositions = LapNumber * 100000 + lastCheckpoint * 1000 - distanceFromNextChkPnt/100;
        // print(distanceInt);
        GameController.instance.kartPositions(kartIndex, playerPositions, 1);
        // print(racePosition);
    }

    void distanceToCheckpoint() //Calculates distance between player and nextcheckpoint, nextCheckpointPos changed onTrigger
    {
        // distanceFromNextChkPnt = Vector3.Distance(rigidBody.position, nextCheckpointPos); //Generates 3KB garbage on 10 bots
        distanceFromNextChkPnt = Vector2.SqrMagnitude(rigidBody.position - nextCheckpointPos);
        if (distanceFromNextChkPnt > highestDistance)
        {
            highestDistance = distanceFromNextChkPnt;
        }
    }

    void matchEnd()
    {
        canMove = false;
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
    void UseSkill()
    {
        if (canUseSkill)
        {
            int direction = 1;    
            if (skillNumber == 0) //Self skill
            {
                //Nitro
                StopNitro();
                StartCoroutine("Nitro");
                return;
            }
            if (skillNumber == 5 || skillNumber == 6) //Backward skills
            {
                direction = -1;
            }
            Skills.instance.getSkill(skillNumber, kartModel.transform.position + kartModel.transform.up * 2f + kartModel.transform.forward * direction * 2, kartModel.rotation.eulerAngles + Vector3.up * (90 * (-1 + direction)), transform.parent);
            canUseSkill = false;
        }
    }

    void startDrift()
    {
        if (canDrift)
        {
            if (drifting)
            {
                StopDrift();
            }
            StartCoroutine("drift");
        }
    }

    IEnumerator drift()
    {
        StartCoroutine(Jump());
        yield return new WaitForSeconds(jumpTime); //Jump airtime
        drifting = true;
        steerAngle = driftingSteerAngle;
        for (int i = 0; i <= Mathf.Round(difficulty/3); i++) //Number of drift boosts in a row (max 2)
        {
            // print("Drift " + i);
            yield return new WaitForSeconds(3f); //drift time before nitro
            canBoost = true;
            StopNitro(); //stops current nitro
            StartCoroutine("Nitro"); //restarts nitro
        }
        // StartCoroutine(driftCD(13 - difficulty * 2)); //Max difficulty = 5, min CD is 3 sec -Don't need with invoke repeating drift
        drifting = false;
    }

    IEnumerator driftCD(float time)
    {
        steerAngle = defaultSteerAngle;
        canDrift = false;
        yield return new WaitForSeconds(time);
        canDrift = true;
    }
}