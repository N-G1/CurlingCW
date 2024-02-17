using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeCamMovement : MonoBehaviour
{
    //TODO only allow freecam to be accessed before stone has been pushed, prob use an enum for 'in play'/'not in play'
    private const float mouseSens = 50;
    private float xRotation = 0, yRotation = 0;
    [SerializeField] private Transform rbHolder;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //Gets horizontal movement, stores in mouseX
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSens * Time.deltaTime;
        //gets vertical movement, stores in mouseY
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSens * Time.deltaTime;

        //collects mouse rotations, to the best of my understanding, rotations take place
        //in the opposite plane, so for example rotating around x takes place in the y plane
        //(in a 2d example) hence why this code works how you would expect 
        yRotation += mouseX;
        xRotation -= mouseY;

        //clamps the rotation to prevent the camera flipping or rotating unnaturally
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    }

    //Rotating done in LateUpdate as rotations done in Update can sometimes be jittery
    private void LateUpdate()
    {
        //rotates the camera by the mouse rotation 
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        rbHolder.rotation = Quaternion.Euler(0, yRotation, 0);

    }
}
