using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCheck : MonoBehaviour
{

    [SerializeField] private Transform centre;

    private List<GameObject> stonesInside = new List<GameObject>();
    private PlayStateManager psm;

    private string closestTeam;
    private int pointsAwarded = 0, prevLayer = -1;
    private bool alreadyChecked = false;

    void Start()
    {
        psm = PlayStateManager.PSMInstance;
    }
    void Update()
    {
        //when a round ends, check for the winning team and pass the vals to the psm for handling with 
        //game state changing 
        if (psm.GetPlayState() == PlayStateManager.PlayStates.RoundEnded && !alreadyChecked)
        {
            if (stonesInside.Count > 0)
            {
                HandlePointCheck();
                PassValsToPSM(pointsAwarded, closestTeam);
            }
            else
            {
                PassValsToPSM(0, "None");
            }
            alreadyChecked = true; 
        }
        //if has been round end, clear everything 
        if (psm.GetPrevPlayState() == PlayStateManager.PlayStates.RoundEnded)
        {
            alreadyChecked = false;
            pointsAwarded = 0;
            stonesInside.Clear();
        }
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Stone") || collision.gameObject.CompareTag("CurrStone"))
        {
            stonesInside.Add(collision.gameObject);      
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("Stone") || collision.gameObject.CompareTag("CurrStone"))
        {
            stonesInside.Remove(collision.gameObject);
        }
    }


    //had to do some research on how I could best implement this,
    //concluded that a simple custom comparator would be best to order the list by 
    //distance to the centre 
    int HandlePointCheck()
    {
        stonesInside.Sort((stone1, stone2) => CmpDist(stone1, stone2));

        for (int i = 0; i <= stonesInside.Count - 1; i++)
        {
            if (i == 0)
            {
                pointsAwarded += 1;
                prevLayer = stonesInside[i].layer;
            }
            else if (stonesInside[i].layer == prevLayer)
            {
                pointsAwarded += 1;
            }
            else
            {
                break;
            }

        }

        closestTeam = prevLayer == 8 ? "Player" : "Opponent";
        return pointsAwarded;
    }

    //custom comparator as mentioned above 
    int CmpDist(GameObject stone1, GameObject stone2)
    {
        //temporary fix for before changing physics
        float dist1 = Vector3.Distance(stone1.transform.GetChild(1).position, centre.position);
        float dist2 = Vector3.Distance(stone2.transform.GetChild(1).position, centre.position);

        if (dist1 < dist2)
        {
            return -1;
        }
        else if (dist1 > dist2)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Passes vals to PlayStateManager for display 
    /// </summary>
    /// <param name="points">points scored</param>
    /// <param name="team">team name</param>
    void PassValsToPSM(int points, string team)
    {
        psm.SetRoundPoints(points);
        psm.SetRoundWinnignTeam(team);
    }
}
