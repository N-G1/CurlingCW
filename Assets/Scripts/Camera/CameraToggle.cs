using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraToggle : MonoBehaviour
{

    [SerializeField] private Camera freeCam;
    private Camera mainCam;
    private bool mainIsEnabled = true;
    private GameStateManager gsm;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        gsm = GameStateManager.GSMInstance;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y) && (gsm.getCurrState() == GameStateManager.MenuStates.Play))
        {
            mainIsEnabled = !mainIsEnabled;
            mainCam.enabled = mainIsEnabled;
            freeCam.enabled = !mainIsEnabled;
        }
    }
}
