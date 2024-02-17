using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//script that manages what to do in the two different types of play (aiming / moving)
public class PlayStateManager : MonoBehaviour
{
    [SerializeField] private GameObject arrow;

    //aiming = initial direction with arrow, directing = moving stone in play 
    public enum PlayStates { Directing, Aiming, EnemyTurn }
    private PlayStates currPlayState;
    private PlayStates prevPlayState;
    private MeshRenderer[] arrowMeshes;
    public static PlayStateManager PSMInstance;

    //Singleton used same as GSM to access in other scripts, not sure if this is the most 
    //efficient way to do this but it works 
    void Awake()
    {
        if (PSMInstance == null)
        {
            PSMInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        currPlayState = PlayStates.Aiming;
    }

    void Start()
    {
        arrowMeshes = arrow.GetComponentsInChildren<MeshRenderer>();
    }

    void Update()
    {
        switch (currPlayState)
        {
            case PlayStates.Aiming:
                changeArrowVisibility(true);
                break;
            case PlayStates.Directing:
                changeArrowVisibility(false);
                break;
            case PlayStates.EnemyTurn:
                break;
        }
    }

    //hide directing arrow after shot has been taken, display it otherwise
    void changeArrowVisibility(bool display)
    {
        foreach (MeshRenderer currRend in arrowMeshes)
        {
            currRend.enabled = display;
        }
    }

    public void setPlayState(PlayStates input)
    {
        currPlayState = input;
    }
    public PlayStates getPlayState()
    {
        return currPlayState;
    }

    public void setPrevPlayState(PlayStates input)
    {
        prevPlayState = input;
    }
    public PlayStates getPrevPlayState()
    {
        return prevPlayState;
    }
}
