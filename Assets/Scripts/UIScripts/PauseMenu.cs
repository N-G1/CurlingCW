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
        gsm.SetCurrState(GameStateManager.MenuStates.Play);
        gsm.SetPrevState(GameStateManager.MenuStates.Pause);
    }

    public void btnMenu()
    {
        gsm.SetCurrState(GameStateManager.MenuStates.MainMenu);
        Time.timeScale = 1;

    }
}
