using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    [SerializeField] Canvas options, menu;
    void Start()
    {
        //init settings if the player has never played 
        if (!(PlayerPrefs.HasKey("PrevLoaded")))
        {
            PlayerPrefs.SetInt("PrevLoaded", 1);
            PlayerPrefs.SetInt("AIDifficulty", 2);
            PlayerPrefs.SetInt("Ends", 3);
            PlayerPrefs.SetInt("Stones", 4);
            PlayerPrefs.SetFloat("Volume", 0.10f);
        }
    }
    public void btnPlay()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public void btnOptions()
    {
        menu.enabled = false;
        options.enabled = true;
    }

    public void btnQuit()
    {
        Application.Quit();
    }
}
