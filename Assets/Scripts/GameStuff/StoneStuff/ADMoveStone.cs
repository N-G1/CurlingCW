using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ADMoveStone : MonoBehaviour
{
    private PlayStateManager psm;
    private GameObject currStone;
    private Rigidbody currStoneRb;

    private float timeControlling = 0f;
    private float timeLimit = 4f;

    private bool cntrlsActive = true;

    private const float movementModifier = 1.3f; //modifier for x plane movement

    void Start()
    {
        psm = PlayStateManager.PSMInstance;
    }

    // Update is called once per frame
    void Update()
    {
        if (psm.getPlayState() == PlayStateManager.PlayStates.Directing && psm.getPrevPlayState() != PlayStateManager.PlayStates.EnemyTurn)
        {
            //pretty ineficient, TODO change 
            currStone = GameObject.FindGameObjectWithTag("CurrStone");
            currStoneRb = currStone.GetComponent<Rigidbody>();

            timeControlling += Time.deltaTime;

            //prevents the player from holding A and D and never moving to the next stone
            if (timeControlling < timeLimit && cntrlsActive == true)
            {
                float horizInput = Input.GetAxis("Horizontal");

                //apply the force only in the x plane
                Vector3 movementDirection = new Vector3(-horizInput, 0f, 0f).normalized;

                //currStone.transform.Rotate(Vector3.forward, horizInput * 3f * Time.deltaTime);
                //rotate the camera in the opposite direction to prevent it moving with the stone
                //currStone.transform.GetChild(0).transform.Rotate(Vector3.back, horizInput * 3f * Time.deltaTime);

                currStoneRb.AddForce(movementDirection * movementModifier);
            } 
            else
            {
                cntrlsActive = false;
            }
        }
        //reactivates controls once new stone is being aimed 
        if (psm.getPlayState() == PlayStateManager.PlayStates.Aiming)
        {
            cntrlsActive = true;
            timeControlling = 0f;
        }
    }
}
