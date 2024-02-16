using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CrowdMember : MonoBehaviour
{
    private bool isMoving = false;

    public bool getIsMoving()
    {
        return isMoving;
    }

    public void setIsMoving(bool val)
    {
        isMoving = val;
    }
}
