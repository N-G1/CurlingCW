using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


//Handles moving the stone in the direction selected
public class GameLoop : MonoBehaviour
{
    [SerializeField] private GameObject stoneSpawnPoint;
    [SerializeField] private GameObject stoneHolder;
    [SerializeField] private GameObject[] stonePrefabs;

    [SerializeField] private Transform pivot;
    [SerializeField] private Transform cameraPos;
    [SerializeField] private Transform target;
    

    [SerializeField] private TextMeshProUGUI txtTurn;

    [SerializeField] private AudioSource camAudioSource;

    private Transform stoneCentre;

    private Camera mainCam;
    private GameObject stone;
    private Rigidbody stoneRb;

    private float velocityTimeLimit = 0.5f;
    private float timeUnderVelocity = 0f;

    private int teamInPlay = 1;

    private bool moving = false;
    private bool ended = false;
    private bool enemyFired = false, enemyActive = false;

    private const float slideModifier = 0.05f; //small modifier applied to simulate longer sliding
    private float velocityModifier = 13f; //initial force applied
    private float currentSpeed = 0f;
    private Vector3 currentVelocity;

    public static GameLoop GLInstance;
    private PlayStateManager psm;
    private GameStateManager gsm;

    private void Awake()
    {
        if (GLInstance == null)
        {
            GLInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //init stone, stone rb and camera
    void Start()
    {
        mainCam = Camera.main;
        mainCam.transform.position = cameraPos.position;
        mainCam.transform.rotation = cameraPos.rotation;

        psm = PlayStateManager.PSMInstance;
        gsm = GameStateManager.GSMInstance;

        stone = GameObject.FindGameObjectWithTag("CurrStone");
        stoneRb = stone.GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Update function plays the stone moving effect, checks if the game has ended
    /// and determines what to do based on which team is in play 
    /// </summary>
    void Update()
    {
        ended = CheckGameEnded();
        
        //why does this work, surely the equality should be the other way around
        if ((currentSpeed < 0.9f && teamInPlay == 1) || (stoneRb.velocity.magnitude < 0.03f && teamInPlay == 2))
        {
            camAudioSource.loop = true;
            camAudioSource.Play();
        }

        if (!ended)
        {
            txtTurn.text = teamInPlay == 1 ? "Red Team's turn" : "Blue Team's turn";

            if (psm.getPlayState() != PlayStateManager.PlayStates.EnemyTurn && teamInPlay == 1 && gsm.getCurrState() != GameStateManager.MenuStates.Pause)
            {
                enemyFired = false;
                enemyActive = false;
                moveStone();
                velocityCheck();
            }
            else
            {
                StartCoroutine(AIAimingDecision());
                if (enemyFired)
                {
                    velocityCheck();
                }          
            }  
        }
    }

    //apply slide modifier
    void FixedUpdate()
    {
        if (moving)
        {
            stoneRb.AddForce(-transform.forward * slideModifier, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// function that handles the player moving the stone
    /// </summary>
    private void moveStone()
    {
        //if user clicks and is aiming 
        if (Input.GetMouseButtonDown(0) && psm.getPlayState() == PlayStateManager.PlayStates.Aiming && Camera.main == mainCam)
        {
            psm.setPlayState(PlayStateManager.PlayStates.Directing);
            psm.setPrevPlayState(PlayStateManager.PlayStates.Aiming);
            moving = true;
            StartCoroutine(handlePhysics());      
        }
    }

    /// <summary>
    /// If stone velocity is almost none, spawn new stone and move over camera, 
    /// handles switching between players turn and opponent turn as well as the round ending
    /// </summary>
    private void velocityCheck()
    {
        if (((currentSpeed < 0.9f && teamInPlay == 1) || (stoneRb.velocity.magnitude < 0.03f && teamInPlay == 2)) && psm.getPlayState() == PlayStateManager.PlayStates.Directing)
        {
            timeUnderVelocity += Time.deltaTime;

            camAudioSource.Stop();
            //check for if velocity has been under 0.03f for more than 0.5 seconds,
            //prevents instantly changing stone if it briefly stops 
            if (timeUnderVelocity > velocityTimeLimit)
            {
                moving = false;
                stone.tag = "Stone";

                //For reference below: Aiming -> Directing -> EnemyTurn -> Directing -> Aiming -> ....

                //if youve just come from aiming then its your turn directing 
                if (psm.getPrevPlayState() == PlayStateManager.PlayStates.Aiming || psm.getPrevPlayState() == PlayStateManager.PlayStates.RoundEnded)
                {
                    stone = Instantiate(stonePrefabs[1], stoneSpawnPoint.transform.position, Quaternion.identity, stoneHolder.transform);
                    psm.setPlayState(PlayStateManager.PlayStates.EnemyTurn); 
                    teamInPlay = 2;
                }
                //if youve just come from enemy turn then its other teams turn directing
                else if (psm.getPrevPlayState() == PlayStateManager.PlayStates.EnemyTurn && psm.getPlayState() != PlayStateManager.PlayStates.RoundEnded)
                {
                    stone = Instantiate(stonePrefabs[0], stoneSpawnPoint.transform.position, Quaternion.identity, stoneHolder.transform);
                    psm.setPlayState(PlayStateManager.PlayStates.Aiming);
                    teamInPlay = 1;
                }
                
                stone.tag = "CurrStone";
                stoneRb = stone.GetComponent<Rigidbody>();

                mainCam.transform.position = stone.transform.GetChild(0).position;
                mainCam.transform.parent = stone.transform.GetChild(0);

                timeUnderVelocity = 0f;
            }   
        }
    }

    /// <summary>
    /// Checks if the game has ended
    /// </summary>
    /// <returns></returns>
    private bool CheckGameEnded()
    {
        //Check when movement has stopped on the final stone of a round
        if (psm.getStonesUsed() == psm.stoneLimit && teamInPlay == 1 && moving == false)
        {
            psm.setPlayState(PlayStateManager.PlayStates.RoundEnded);
            psm.setPrevPlayState(PlayStateManager.PlayStates.Directing);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Applies force to stone TODO:redo physics 
    /// </summary>
    private IEnumerator handlePhysics()
    {
        float timeControlling = 0f;
        float timeLimit = 4f;
        Vector3 nextPos = stone.transform.position, prevPos = stone.transform.position;
        //pivot is facing backwards   
        Vector3 direction = -pivot.forward;
        velocityModifier = 13f;

        while (moving && velocityModifier >= 0.85f)
        {
            //prefab is off centre, so instead rotate around child gameobject at centre of pivot 
            stoneCentre = stone.transform.GetChild(1).transform;
            timeControlling += Time.deltaTime;

            //prevents the player from holding A and D and never moving to the next stone
            if (timeControlling < timeLimit && (Input.GetKey(KeyCode.A) || (Input.GetKey(KeyCode.D))))
            {
                float horizInput = Input.GetAxis("Horizontal");
                //both below meant to simulate reducting in rotation/sweeping speed as stone slows down 
                float rotModifier = currentSpeed < 2f ? 0.6f : currentSpeed < 4f ? 1f : 1.3f;
                float sweepModifier = currentSpeed < 3f ? 1f : currentSpeed < 8f ? 2f : 3f;

                //apply the force only in the x plane
                Vector3 movDirection = new Vector3(-horizInput, 0f, 0f).normalized;
                movDirection.x = direction.x;

                //rotate the stone around the central gameobject, rotate camera in opposite direction so it stays stationary
                stone.transform.RotateAround(stoneCentre.position, Vector3.up, horizInput * (10f * rotModifier) * Time.deltaTime);
                stone.transform.GetChild(0).transform.RotateAround(stoneCentre.position, Vector3.down, horizInput * (10f * rotModifier) * Time.deltaTime);

                prevPos = stone.transform.position;
                nextPos = prevPos + movDirection * sweepModifier * Time.deltaTime;

                //interpolates between previous direction and new direction, more realistic turning when switching from pivot direction
                direction = Vector3.Slerp(direction, -stoneCentre.transform.forward, 0.01f);

                //currStoneRb.AddForce(movDirection * movementModifier);
            }
            prevPos = stone.transform.position;
            nextPos = prevPos + direction * velocityModifier * Time.deltaTime;

            //workout speed for stone stopping and turning
            currentSpeed = (nextPos - prevPos).magnitude / Time.deltaTime;
            currentVelocity = (nextPos - prevPos) / Time.deltaTime;

            stone.transform.position = nextPos;
            //gradually decrease velocity
            velocityModifier *= 0.9973f;
            yield return null;    
        }
    }

    /// <summary>
    /// AI physics works with a ridgidbody instead of manual physics 
    /// </summary>
    private void handleAIPhysics()
    {
        velocityModifier = 13f;
        moving = true;
        //pivot is facing backwards
        Vector3 direction = -pivot.forward;
        stoneRb.velocity = direction * velocityModifier;
    }

    /// <summary>
    /// Handles the AI's decision making on what angle to slide the stone
    /// Coroutine due to periodic wait between angle checks to simulate waiting/thought
    /// </summary>
    /// <returns></returns>
    private IEnumerator AIAimingDecision()
    {
        //flag to ensure only 1 coroutine is running
        if (!enemyActive && gsm.getCurrState() != GameStateManager.MenuStates.Pause)
        {
            enemyActive = true;
            float maxAngle = 0;
            float firingChance = 0f;

            while (!enemyFired)
            {
                yield return new WaitForSeconds(0.5f);
                //calculate the angle between where the arrow pivot is and the target
                float angle = Vector3.Angle(pivot.forward, pivot.position - target.position);
                Debug.Log("Angle is " + angle);

                maxAngle = PlayerPrefs.GetInt("AIDifficulty") == 1 ? 45 : 
                           PlayerPrefs.GetInt("AIDifficulty") == 2 ? 25 : 18;

                //Equation I trial and errored until I found something I liked
                //nicely increases the chance as the angle approaches 0, however chance         e^((-x+0.001) / maxAngle * 1 - x * 0.02f) where x <= 45/25/15 and x = angle
                //got too high as it approached 0, so cut off at 80% chance to slide
                firingChance = Mathf.Min(Mathf.Exp((-angle + 0.001f) / maxAngle * 1 - angle * 0.02f), 0.80f);

                //if there are any blocking stones, decrease the chance of the AI firing directly straight
                if (CenterCollider.stonesInCenter.Count > 0 && angle <= 6)
                {
                    firingChance *= 0.5f;
                }
                //if the angle is still reasonable (but not straight) and there are blocking stones, increase chance to slide at these angles
                else if (CenterCollider.stonesInCenter.Count > 0 && angle >= 6 && angle <= 20)
                {
                    firingChance += firingChance * 0.25f;
                }

                if (Random.Range(0f, 1.01f) < firingChance)
                {
                    handleAIPhysics();
                    enemyFired = true;

                    //set the states and manage directing once the enemy has selected an angle to slide 
                    psm.setPlayState(PlayStateManager.PlayStates.Directing);
                    psm.setPrevPlayState(PlayStateManager.PlayStates.EnemyTurn);
                    StartCoroutine(AIDirectingDecisions());
                }
            }
        }    
    }

    /// <summary>
    /// Decition making of how the AI controls the stone after the initial slide 
    /// </summary>
    private IEnumerator AIDirectingDecisions()
    {
        bool stoneMoved = false;
        while (moving)
        {
            //Get the x cords of both the stone and the target
            Vector3 stoneXCoords = new Vector3(stone.transform.position.x, 0f, 0f);
            Vector3 targetXCoords = new Vector3(target.position.x, 0f, 0f);

            //Works out the distance between the stone and target in the X plane, used below to check 
            //if it is outside of bounds for curling
            float distanceInX = Vector3.Distance(stoneXCoords, targetXCoords);

            string sideOfTarget;

            //if the x coordinate of the target is smaller than the x of the stone
            //then the stone is on the right, else the left
            if (target.position.x < stone.transform.position.x)
            {
                sideOfTarget = "right";
            }
            else
            {
                sideOfTarget = "left";
            }

            //prevents the stone from constantly being moved
            if (stoneMoved)
            {
                yield return new WaitForSeconds(0.05f);
                stoneMoved = false;
            }

            stoneMoved = true;

            //if stone is not on course, allow AI to curl
            if (distanceInX > 0.4f)
            {
                AICurling(sideOfTarget);
            }
        }
    }

    void AICurling(string sideOfTarget)
    {
        float horizInput = (sideOfTarget == "left") ? -1f : 1f;

        //apply the force only in the x plane
        Vector3 movementDirection = new Vector3(-horizInput, 0f, 0f).normalized;

        stoneRb.AddForce(movementDirection * 5f);
        Debug.Log("curled from: " + sideOfTarget );
    }

    /// <summary>
    /// Returns ended boolean
    /// </summary>
    /// <returns>ended</returns>
    public bool getEnded()
    {
        return ended;
    }
    /// <summary>
    /// Sets ended boolean
    /// </summary>
    /// <param name="val">boolean</param>
    public void setEnded(bool val)
    {
        ended = val;
    }

    public float getCurrSpeed()
    {
        return currentSpeed;
    }
    public void setCurrSpeed(float speed)
    {
        currentSpeed = speed;
    }
    public Vector3 getCurrVelocity()
    {
        return currentVelocity;
    }
    public void setCurrVelocity(Vector3 vec)
    {
        currentVelocity = vec;
    }
}
