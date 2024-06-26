using System.Collections;
using UnityEngine;

public class CrowdMovement : MonoBehaviour
{
    [SerializeField] private LayerMask ignoredLayer;

    void Start()
    {
        GameObject[] crowd = GameObject.FindGameObjectsWithTag("Crowd");
        foreach(GameObject crowdMember in crowd)
        {
            StartCoroutine(MoveCrowdMember(crowdMember));
        }
    }

    //Simple crowd movement coroutine, so they don't all jump in a uniform manner
    IEnumerator MoveCrowdMember(GameObject crowdMember)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0.1f, 2f));
            
            //Simple raycast to check if crowd member is on the ground, ignores the crowd layer using not operator 
            if (Physics.Raycast(crowdMember.transform.position, Vector3.down, 0.6f, ~ignoredLayer))
            {
                crowdMember.GetComponent<Rigidbody>().velocity = new Vector3(0, Random.Range(2.5f, 5f), 0);
            }
             
        }
    }

}
