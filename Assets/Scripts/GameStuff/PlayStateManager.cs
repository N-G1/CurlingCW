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
    private GameLoop gl;

    private MeshRenderer[] arrowMeshes;
    public static PlayStateManager PSMInstance;
    

    private int roundPoints = 0, stonesUsed = 0;
    private int playerPoints, enemyPoints = 0;
    private string roundWinningTeam, winningTeam;
    private bool incrementedPoints = false;
    private GameStateManager gsm;
    private bool hasBeenSet = false;
    private bool stoneHasBeenIncremented = false;


    //number of stones each team has each round 
    public const int stoneLimit = 1;
    //number of ends 
    private int ends = 2, endsPlayed = 0;


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
        winningTeam = "None";
    }

    void Start()
    {
        arrowMeshes = arrow.GetComponentsInChildren<MeshRenderer>();
        gl = GameLoop.GLInstance;
        gsm = GameStateManager.GSMInstance;
    }

    void Update()
    {
        switch (currPlayState)
        {
            case PlayStates.Aiming:
                ChangeArrowVisibility(true);
                incrementedPoints = false;
                hasBeenSet = false;
                stoneHasBeenIncremented = false;
                break;
            case PlayStates.Directing:
                ChangeArrowVisibility(false);
                break;
            case PlayStates.EnemyTurn:
                if (!stoneHasBeenIncremented)
                {
                    setStonesUsed(getStonesUsed() + 1);
                    stoneHasBeenIncremented = true;
                }
                break;
            case PlayStates.RoundEnded:
                //prevents setEndsPlayed being set more than once a round
                IncrementScore();
                if (!hasBeenSet)
                {
                    setEndsPlayed(getEndsPlayed() + 1);
                    hasBeenSet = true;
                }
                if (getEndsPlayed() != ends)
                {
                    RoundEnded();
                }
                else
                {
                    ChangeArrowVisibility(false);
                    gsm.setCurrState(GameStateManager.MenuStates.GameEnded);
                    gsm.setPrevState(GameStateManager.MenuStates.Play);
                }
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

    void IncrementScore()
    {
        //TEMP IMPLEMENTATION - maybe not temp actually 
        if (roundWinningTeam == "Player" && !incrementedPoints)
        {
            playerPoints += getRoundPoints();
            incrementedPoints = true;
        }
        else if (!incrementedPoints)
        {
            enemyPoints += getRoundPoints();
            incrementedPoints = true;
        }
    }

    void RoundEnded()
    {
        gameUI.enabled = false;
        roundUI.enabled = true;

        if (playerPoints != 0 || enemyPoints != 0)
        {
            winningTeam = playerPoints > enemyPoints ? "Player" : "Enemy";
        }

        txtTemp.text = string.Format("{0}: {1}", winningTeam, playerPoints);

        IncrementScore();
        
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

        //set gameEnded as false so gameloop can resume
        gl.setEnded(false);
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
    public void setRoundWinnignTeam(string input)
    {
        roundWinningTeam = input;
    }
    public string getRoundWinningTeam()
    {
        return roundWinningTeam;
    }
    public void setStonesUsed(int input)
    {
        stonesUsed = input;
    }
    public int getStonesUsed()
    {
        return stonesUsed;
    }
    public void setEndsPlayed(int input)
    {
        endsPlayed = input;
    }
    public int getEndsPlayed()
    {
        return endsPlayed;
    }
    public int getPlayerPoints()
    {
        return playerPoints;
    }
    public int getEnemyPoints()
    {
        return enemyPoints;
    }
}
