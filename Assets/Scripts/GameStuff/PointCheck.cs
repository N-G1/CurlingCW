using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCheck : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Stone" || collision.gameObject.tag == "CurrStone")
        {

        }
    }

    //overlap collider to check number of colliders within collider box, then, compare the tag which will be either 
    //stone or currstone, and then check the layer of that stone, playerTeam and EnemyTeam, perform a check to see 
    //which is closest to the centre, if no colliders are within the collider box at the end of a round, award no points
}
