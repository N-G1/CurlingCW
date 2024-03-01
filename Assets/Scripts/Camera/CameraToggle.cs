using UnityEngine;

public class CameraToggle : MonoBehaviour
{

    [SerializeField] private Camera freeCam;
    private Camera mainCam;
    private bool mainIsEnabled = true;
    private GameStateManager gsm;

    void Start()
    {
        mainCam = Camera.main;
        gsm = GameStateManager.GSMInstance;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y) && (gsm.GetCurrState() == GameStateManager.MenuStates.Play))
        {
            mainIsEnabled = !mainIsEnabled;
            mainCam.enabled = mainIsEnabled;
            freeCam.enabled = !mainIsEnabled;
        }
    }
}
