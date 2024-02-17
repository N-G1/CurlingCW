using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour
{
    [SerializeField] private Transform cameraPos;
    private Camera mainCam;

    // Initialize camera position
    void Start()
    {
        mainCam = Camera.main;
        mainCam.transform.position = cameraPos.position;
        mainCam.transform.rotation = cameraPos.rotation;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
