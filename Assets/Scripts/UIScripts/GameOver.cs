using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScript : MonoBehaviour
{
    public void PlayAgain()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
    public void Menu()
    {
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
}
