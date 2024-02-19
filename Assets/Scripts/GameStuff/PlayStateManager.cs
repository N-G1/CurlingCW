using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


//script that manages what to do in the different stages of play 
public class PlayStateManager : MonoBehaviour
{
    [SerializeField] private GameObject arrow;
    //despite the fact all other UI's are handled in the gsm, I think 
    //that the round ending fits more as part of the actual game loop
    [SerializeField] private Canvas roundUI;
    [SerializeField] private Canvas gameUI;
    //TEMP IMPLEMENTATION
    [SerializeField] private TextMeshProUGUI txtTemp;

    //aiming = initial direction with arrow, directing = moving stone in play 
    public enum PlayStates { Directing, Aiming, EnemyTurn, RoundEnded }
    private PlayStates currPlayState;
    private PlayStates prevPlayState;

    private MeshRenderer[] arrowMeshes;
    public static PlayStateManager PSMInstance;
    private int roundPoints = 0, stonesUsed = 0;
    private string winningTeam;

    //number of stones each team has each round 
    public const int stoneLimit = 3;


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
                ChangeArrowVisibility(true);
                break;
            case PlayStates.Directing:
                ChangeArrowVisibility(false);
                break;
            case PlayStates.EnemyTurn:
                ChangeArrowVisibility(false);
                //as enemyTurn will be last before round end, increment here 
                setStonesUsed(getStonesUsed() + 1);
                break;
            case PlayStates.RoundEnded:
                RoundEnded();
                //display rounded ended UI for 3 seconds, disable controls 
                break;
        }
    }

    /// <summary>
    /// Hide directing arrow after shot has been taken, display it otherwise
    /// </summary>
    /// <param name="display"> bool to enable or disable</param>
    void ChangeArrowVisibility(bool display)
    {
        foreach (MeshRenderer currRend in arrowMeshes)
        {
            currRend.enabled = display;
        }
    }

    void RoundEnded()
    {
        gameUI.enabled = false;
        roundUI.enabled = true;

        //TEMP IMPLEMENTATION
        txtTemp.text = (winningTeam + ": " + roundPoints);

        //wait 3 seconds to view leaderboard then start new round 
        StartCoroutine(NewRound());
    }

    /// <summary>
    /// Handles changing states to new round + enabling / disabling UI
    /// </summary>
    IEnumerator NewRound()
    {
        setPlayState(PlayStates.Aiming);
        setPrevPlayState(PlayStates.RoundEnded);

        yield return new WaitForSecondsRealtime(3);

        DestroyStones();

        gameUI.enabled = true;
        roundUI.enabled = false;

        setRoundPoints(0);
        setStonesUsed(0);
    }

    /// <summary>
    /// Destroy prev round stones (all but currStone)
    /// </summary>
    void DestroyStones()
    {
        GameObject[] stones = GameObject.FindGameObjectsWithTag("Stone");
        foreach (GameObject stone in stones)
        {
            Destroy(stone);
        }
    }


    //Getters and setters below, not implemented via { get; set } as used in other scripts 
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

    public void setRoundPoints(int input)
    {
        roundPoints = input;
    }
    public int getRoundPoints()
    {
        return roundPoints;
    }
    public void setWinnignTeam(string input)
    {
        winningTeam = input;
    }
    public string getWinningTeam()
    {
        return winningTeam;
    }
    public void setStonesUsed(int input)
    {
        stonesUsed = input;
    }
    public int getStonesUsed()
    {
        return stonesUsed;
    }
}
