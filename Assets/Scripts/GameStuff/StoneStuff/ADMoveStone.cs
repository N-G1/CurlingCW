using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ADMoveStone : MonoBehaviour
{
    private PlayStateManager psm;
    private GameObject currStone;
    private Transform stoneCentre;
    private Rigidbody currStoneRb;

    private float timeControlling = 0f;
    private float timeLimit = 4f;

    //private bool cntrlsActive = true;

    void Start()
    {
        psm = PlayStateManager.PSMInstance;
    }

    // Update is called once per frame
    void Update()
    {
        if (psm.getPlayState() == PlayStateManager.PlayStates.Directing && psm.getPrevPlayState() != PlayStateManager.PlayStates.EnemyTurn)
        {
            currStone = GameObject.FindGameObjectWithTag("CurrStone");
            //prefab is off centre, so instead rotate around child gameobject at centre of pivot 
            stoneCentre = currStone.transform.GetChild(1).transform; 
            currStoneRb = currStone.GetComponent<Rigidbody>();

            timeControlling += Time.deltaTime;

            //prevents the player from holding A and D and never moving to the next stone
            if (timeControlling < timeLimit && (Input.GetKey(KeyCode.A) || (Input.GetKey(KeyCode.D))))
            {
                float horizInput = Input.GetAxis("Horizontal");
                float magnitude = currStoneRb.velocity.magnitude; 

                //both below meant to simulate reducting in rotation/sweeping speed increase as stone slows down 
                float rotModifier = magnitude < 2f ? 0.6f : magnitude < 4f ? 1f : 1.3f;
                float movementModifier = magnitude < 3f ? 1.15f : magnitude < 8f ? 1.35f : 1.45f; 

                Vector3 movDirection;

                //apply the force only in the x plane
                movDirection = new Vector3(-horizInput, 0f, 0f).normalized;

                //Quaternion targetRotation = Quaternion.LookRotation(currStone.transform.forward);
                //stoneCentre.localRotation = Quaternion.Slerp(stoneCentre.localRotation, targetRotation, 10f * Time.deltaTime);

                //rotate the stone around the central gameobject, rotate camera in opposite direction so it stays stationary
                currStone.transform.RotateAround(stoneCentre.position, Vector3.up, horizInput * (10f * rotModifier) * Time.deltaTime);
                currStone.transform.GetChild(0).transform.RotateAround(stoneCentre.position, Vector3.down, horizInput * (10f * rotModifier) * Time.deltaTime);

                currStoneRb.AddForce(movDirection * movementModifier);
            } 
        }
        if (psm.getPlayState() == PlayStateManager.PlayStates.Aiming)
        {
            timeControlling = 0f;
        }
    }
}
