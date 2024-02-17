using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    private GameStateManager gsm;
    private void Start()
    {
        gsm = GameStateManager.GSMInstance;
    }
    public void btnContinue()
    {
        gsm.setCurrState(GameStateManager.MenuStates.Play);
        gsm.setPrevState(GameStateManager.MenuStates.Pause);
    }

    public void btnMenu()
    {
        gsm.setCurrState(GameStateManager.MenuStates.MainMenu);
        Time.timeScale = 1;

    }
}
