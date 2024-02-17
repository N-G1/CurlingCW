using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    //TODO add functionality to remember last screen and reformat 
    [SerializeField] private GameObject gameEndedUI, gameUI, pauseUI;

    public enum MenuStates { MainMenu, GameEnded, Pause, Play }
    private MenuStates currState, prevState;
    public static GameStateManager GSMInstance;
    //unload stones on gameended, load them on play, so never loaded on main menu 

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

    }
    private void PauseMenu()
    {
        CursorUnlock();
        pauseUI.GetComponent<Canvas>().enabled = true;
        gameUI.GetComponent<Canvas>().enabled = false;
        Time.timeScale = 0;
    }

    private void Play()
    {
        CheckPrevScreen();
        CursorLock();
        gameUI.GetComponent<Canvas>().enabled = true;
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
                pauseUI.GetComponent<Canvas>().enabled = false;
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
