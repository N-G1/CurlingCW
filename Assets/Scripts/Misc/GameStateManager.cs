using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameStateManager : MonoBehaviour
{
    //TODO add functionality to remember last screen and reformat 
    [SerializeField] private Canvas gameEndedUI, gameUI, pauseUI;
    [SerializeField] private TextMeshProUGUI txtFinalDisplay;

    public enum MenuStates { MainMenu, GameEnded, Pause, Play }
    private MenuStates currState, prevState;
    public static GameStateManager GSMInstance;
    private PlayStateManager psm;

    //singleton used to access the gsm in other scripts via instance 
    void Awake()
    {
        if (GSMInstance == null)
        {
            GSMInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        currState = MenuStates.Play;
    }

    // Start is called before the first frame update
    void Start()
    {
        psm = PlayStateManager.PSMInstance;
    }

    // Update is called once per frame
    void Update()
    {
        //check if already paused and do different things depending 
        if (Input.GetKeyDown(KeyCode.Escape) && currState == MenuStates.Play)
        {
            setCurrState(MenuStates.Pause);
            //Prev state only needs to be set when coming from game ended or paused
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && currState == MenuStates.Pause)
        {
            setCurrState(MenuStates.Play);
            setPrevState(MenuStates.Pause);
        }

        //enum for managing different states of play 
        switch (currState)
        {
            case MenuStates.MainMenu:
                CursorUnlock();
                MainMenu();
                break;
            case MenuStates.GameEnded:
                CursorUnlock();
                GameEnded();
                break;
            case MenuStates.Pause:
                CursorUnlock();
                PauseMenu();
                break;
            case MenuStates.Play:
                Play();
                break;
        }
    }


    private void MainMenu()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
    private void GameEnded()
    {
        gameEndedUI.enabled = true;
        gameUI.enabled = false;
        txtFinalDisplay.text = string.Format("Player: {0}\nOpponent: {1}", psm.getPlayerPoints(), psm.getEnemyPoints());
    }
    private void PauseMenu()
    {
        CursorUnlock();
        pauseUI.enabled = true;
        gameUI.enabled = false;
        Time.timeScale = 0;
    }

    private void Play()
    {
        CheckPrevScreen();
        CursorLock();
        gameUI.enabled = true;
    }

    //Unlocks cursor
    private void CursorUnlock()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //Locks cursor
    private void CursorLock()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    //checks the previous screen and hides the relevant UI
    private void CheckPrevScreen()
    {
        switch (prevState)
        {
            case MenuStates.GameEnded:
                //
                break;
            case MenuStates.Pause:
                Time.timeScale = 1;
                pauseUI.enabled = false;
                break;
        }
    }

    public void setCurrState(MenuStates input)
    {
        currState = input;
    }
    public MenuStates getCurrState()
    {
        return currState;
    }
    public void setPrevState(MenuStates input)
    {
        prevState = input;
    }
    public MenuStates getPrevState()
    {
        return prevState;
    }
}
