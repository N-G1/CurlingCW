using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public enum MenuStates { MainMenu, Options, GameEnded, Pause, Play }
    public MenuStates currState;
    //unload stones on gameended, load them on play, so never loaded on main menu 

    private void Awake()
    {
        currState = MenuStates.MainMenu;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
