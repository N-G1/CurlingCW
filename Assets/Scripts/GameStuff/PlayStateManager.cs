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
    [SerializeField] private Canvas roundUI, gameUI;
    [SerializeField] private TextMeshProUGUI txtTemp;
    [SerializeField] private AudioSource camAudio;
    [SerializeField] private AudioClip sfxDisappointed, sfxApplause;

    //aiming = initial direction with arrow, directing = moving stone in play 
    public enum PlayStates { Directing, Aiming, EnemyTurn, RoundEnded }
    private PlayStates currPlayState;
    private PlayStates prevPlayState;
    
    private MeshRenderer[] arrowMeshes;

    public static PlayStateManager PSMInstance;
    private GameStateManager gsm;
    private GameLoop gl;

    private int roundPoints = 0, stonesUsed = 0;
    private int playerPoints, enemyPoints = 0;
    private string roundWinningTeam, winningTeam;
    
    private bool incrementedPoints = false;
    private bool hasBeenSet = false;
    private bool stoneHasBeenIncremented = false;
    private bool audioPlayed = false;

    //number of stones each team has each round 
    public int stoneLimit;
    //number of ends 
    private int ends, endsPlayed = 0;

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

        stoneLimit = PlayerPrefs.GetInt("Stones");
        ends = PlayerPrefs.GetInt("Ends");
        camAudio.volume = PlayerPrefs.GetFloat("Volume");
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
                ResetFlags();
                break;
            case PlayStates.Directing:
                ChangeArrowVisibility(false);
                break;
            case PlayStates.EnemyTurn:
                if (!stoneHasBeenIncremented)
                {
                    SetStonesUsed(GetStonesUsed() + 1);
                    stoneHasBeenIncremented = true;
                }
                break;
            case PlayStates.RoundEnded:
                HandleRoundEnd();
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
        if (roundWinningTeam == "Player" && !incrementedPoints)
        {
            playerPoints += GetRoundPoints();
            incrementedPoints = true;
        }
        else if (!incrementedPoints)
        {
            enemyPoints += GetRoundPoints();
            incrementedPoints = true;
        }
        PlaySFX();
    }
    /// <summary>
    /// Plays the relevant sound effect
    /// </summary>
     void PlaySFX()
    {
        if (!audioPlayed)
        {
            if (playerPoints > enemyPoints)
            {
                camAudio.PlayOneShot(sfxApplause);
            }
            else if (enemyPoints > playerPoints)
            {
                camAudio.PlayOneShot(sfxDisappointed);
            }
            audioPlayed = true;
        }
    }

    /// <summary>
    /// Handles what to do at the end of a round, including incrementing the 
    /// appropriate teams score, and ending the game if neccessary 
    /// </summary>
    void HandleRoundEnd()
    {
        //prevents setEndsPlayed being set more than once a round
        IncrementScore();
        if (!hasBeenSet)
        {
            SetEndsPlayed(GetEndsPlayed() + 1);
            hasBeenSet = true;
        }
        if (GetEndsPlayed() != ends)
        {
            RoundedEndedDisplay();
        }
        else
        {
            ChangeArrowVisibility(false);
            gsm.SetCurrState(GameStateManager.MenuStates.GameEnded);
            gsm.SetPrevState(GameStateManager.MenuStates.Play);
        }
    }

    /// <summary>
    /// Displays the correct summary at the end of a round
    /// </summary>
    void RoundedEndedDisplay()
    {
        gameUI.enabled = false;
        roundUI.enabled = true;
        string points = "";

        if (playerPoints != 0 || enemyPoints != 0)
        {
            winningTeam = playerPoints > enemyPoints ? "Red Team" : "Blue Team";

            //if either team has 1 point and it is the most points, then singular 'point', else multiple 'points'
            points = playerPoints > enemyPoints && playerPoints == 1 ? "point" : 
                     enemyPoints > playerPoints && enemyPoints == 1 ? "point" : "points";
        }
        
        if (playerPoints == 0 && enemyPoints == 0)
        {
            txtTemp.text = "No round winner as no points were scored!";
            
        }
        else
        {
            txtTemp.text = string.Format("Round winner is {0}! with {1} {2}", winningTeam, GetRoundPoints(), points);
        }
        
        //wait 3 seconds to view rounded ended screen then start new round 
        StartCoroutine(NewRound());
    }

    /// <summary>
    /// Handles changing states to new round + enabling / disabling UI
    /// </summary>
    IEnumerator NewRound()
    {
        SetPlayState(PlayStates.Aiming);
        SetPrevPlayState(PlayStates.RoundEnded);
        
        yield return new WaitForSecondsRealtime(2);

        DestroyStones();

        yield return new WaitForSecondsRealtime(1);

        gameUI.enabled = true;
        roundUI.enabled = false;

        SetRoundPoints(0);
        SetStonesUsed(0);

        //set gameEnded as false so gameloop can resume
        gl.SetEnded(false);
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

    /// <summary>
    /// Resets all boolean flags used during play to prevent certain lines and
    /// functions being ran more than once per state
    /// </summary>
    void ResetFlags()
    {
        incrementedPoints = false;
        hasBeenSet = false;
        stoneHasBeenIncremented = false;
        audioPlayed = false;
    }

    //Getters and setters below, not implemented via { get; set } as used in other scripts 
    public void SetPlayState(PlayStates input)
    {
        currPlayState = input;
    }
    public PlayStates GetPlayState()
    {
        return currPlayState;
    }

    public void SetPrevPlayState(PlayStates input)
    {
        prevPlayState = input;
    }
    public PlayStates GetPrevPlayState()
    {
        return prevPlayState;
    }

    public void SetRoundPoints(int input)
    {
        roundPoints = input;
    }
    public int GetRoundPoints()
    {
        return roundPoints;
    }
    public void SetRoundWinnignTeam(string input)
    {
        roundWinningTeam = input;
    }
    public void SetStonesUsed(int input)
    {
        stonesUsed = input;
    }
    public int GetStonesUsed()
    {
        return stonesUsed;
    }
    public void SetEndsPlayed(int input)
    {
        endsPlayed = input;
    }
    public int GetEndsPlayed()
    {
        return endsPlayed;
    }
    public int GetPlayerPoints()
    {
        return playerPoints;
    }
    public int GetEnemyPoints()
    {
        return enemyPoints;
    }
}
