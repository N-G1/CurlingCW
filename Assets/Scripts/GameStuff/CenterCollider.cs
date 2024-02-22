using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterCollider : MonoBehaviour
{
    public static List<GameObject> stonesInCenter = new List<GameObject>();
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Stone" || collision.gameObject.tag == "CurrStone")
        {
            stonesInCenter.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.tag == "Stone" || collision.gameObject.tag == "CurrStone")
        {
            stonesInCenter.Remove(collision.gameObject);
        }
    }
}
