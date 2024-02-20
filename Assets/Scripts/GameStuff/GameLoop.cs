using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Handles moving the stone in the direction selected
public class GameLoop : MonoBehaviour
{
    [SerializeField] private GameObject stoneSpawnPoint;
    [SerializeField] private GameObject stoneHolder;
    [SerializeField] private GameObject[] stonePrefabs;

    [SerializeField] private Transform pivot;
    [SerializeField] private Transform cameraPos;
    [SerializeField] private Transform target;


    private Camera mainCam;
    private GameObject stone;
    private Rigidbody stoneRb;

    private float velocityTimeLimit = 0.5f;
    private float timeUnderVelocity = 0f;

    private int teamInPlay = 1;

    private bool moving = false;
    private bool ended = false;
    private bool enemyFired = false, enemyAimingStarted = false;

    private const float slideModifier = 0.05f; //small modifier applied to simulate longer sliding
    private const float velocityModifier = 13f; //initial force applied

    public static GameLoop GLInstance;
    private PlayStateManager psm;

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
        stone = GameObject.FindGameObjectWithTag("CurrStone");
        stoneRb = stone.GetComponent<Rigidbody>();
    }

    void Update()
    {
        ended = CheckGameEnded();

        if (!ended)
        {
            if (psm.getPlayState() != PlayStateManager.PlayStates.EnemyTurn && teamInPlay == 1)
            {
                enemyFired = false;
                enemyAimingStarted = false;
                moveStone();
                velocityCheck();
            }
            else
            {
                StartCoroutine(AIAimingDecision());
                AIDirectingDecisions();
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
            handlePhysics();      
        }
    }

    /// <summary>
    /// If stone velocity is almost none, spawn new stone and move over camera, 
    /// handles switching between players turn and opponent turn as well as the round ending
    /// </summary>
    private void velocityCheck()
    {
        if (stoneRb.velocity.magnitude < 0.03f && psm.getPlayState() == PlayStateManager.PlayStates.Directing)
        {
            timeUnderVelocity += Time.deltaTime;
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

    private bool CheckGameEnded()
    {
        //Check when movement has stopped on the final stone of a round
        if (psm.getStonesUsed() == PlayStateManager.stoneLimit && teamInPlay == 1 && moving == false)
        {
            psm.setPlayState(PlayStateManager.PlayStates.RoundEnded);
            psm.setPrevPlayState(PlayStateManager.PlayStates.Directing);
            return true;
        }
        return false;
    }

    //new stones are assigned as the primary stone once the previous one has stopped moving
    private void handlePhysics()
    {
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
        if (!enemyAimingStarted)
        {
            enemyAimingStarted = true;
            float firingChance = 0f;

            while (!enemyFired)
            {
                yield return new WaitForSeconds(0.5f);
                //calculate the angle between where the arrow pivot is and the target
                float angle = Vector3.Angle(pivot.forward, pivot.position - target.position);
                Debug.Log("Angle is " + angle);

                //Equation I trial and errored until I found something I liked
                //nicely increases the chance as the angle approaches 0, however chance         e^(((-x+0.001) /60 * 1 - x * 0.015f) where x <= 15 and x = angle
                //got too high as it approached 0, so cut off at 65% chance to slide
                firingChance = Mathf.Min(Mathf.Exp((-angle + 0.001f) / 15f * 1 - angle * 0.09f), 0.80f);

                if (Random.Range(0f, 1.01f) < firingChance)
                {
                    handlePhysics();
                    enemyFired = true;

                    //set the states once the enemy has selected an angle to slide 
                    psm.setPlayState(PlayStateManager.PlayStates.Directing);
                    psm.setPrevPlayState(PlayStateManager.PlayStates.EnemyTurn);
                }
            }
         
        }    
    }

    void AIDirectingDecisions()
    {

    }

    public bool getEnded()
    {
        return ended;
    }
    public void setEnded(bool val)
    {
        ended = val;
    }
}
