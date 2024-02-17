using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Handles moving the stone in the direction selected
public class DecideDirection : MonoBehaviour
{
    [SerializeField] private GameObject stoneSpawnPoint;
    [SerializeField] private GameObject stoneHolder;
    [SerializeField] private Camera mainCam;
    [SerializeField] private Transform pivot;
    [SerializeField] private GameObject stonePrefab;

    private GameObject stone;
    private Rigidbody stoneRb;
    private float velocityTimeLimit = 0.5f;
    private float timeUnderVelocity = 0f;

    private PlayStateManager psm;

    void Start()
    {
        psm = PlayStateManager.PSMInstance;
        stone = GameObject.FindGameObjectWithTag("Stone");
        stoneRb = stone.GetComponent<Rigidbody>();
    }

    void Update()
    {     
        velocityCheck();
        moveStone();
    }

    private void moveStone()
    {
        //if user clicks and is aiming 
        if (Input.GetMouseButtonDown(0) && psm.getPlayState() == PlayStateManager.PlayStates.Aiming)
        {
            psm.setPlayState(PlayStateManager.PlayStates.Directing);
            handlePhysics();      
        }
    }

    //if stone velocity is almost none, spawn new stone and move over camera
    private void velocityCheck()
    {
        if (stoneRb.velocity.magnitude < 0.03f && psm.getPlayState() == PlayStateManager.PlayStates.Directing)
        {
            timeUnderVelocity += Time.deltaTime;
            //check for if velocity has been under 0.03f for more than 0.5 seconds,
            //prevents instantly changing stone if it briefly stops 
            if (timeUnderVelocity > velocityTimeLimit)
            {
                stone = Instantiate(stonePrefab, stoneSpawnPoint.transform.position, Quaternion.identity, stoneHolder.transform);
                stoneRb = stone.GetComponent<Rigidbody>();
                mainCam.transform.position = stone.transform.GetChild(0).position;
                mainCam.transform.parent = stone.transform.GetChild(0);

                //or enemy when other team is implemented
                psm.setPlayState(PlayStateManager.PlayStates.Aiming);
                timeUnderVelocity = 0f;
            }   
        }
    }

    //new stones are assigned as the primary stone once the previous one has stopped moving
    private void handlePhysics()
    {
        //awkward orientation stuff pivot is actually facing backwards for some reason 
        Vector3 direction = -pivot.forward;
        stoneRb.velocity = direction * 15f;
    }
}
