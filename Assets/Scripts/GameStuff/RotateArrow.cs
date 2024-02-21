using UnityEngine;
public class RotateArrow : MonoBehaviour
{
    private PlayStateManager psm;
    void Start()
    {
        psm = PlayStateManager.PSMInstance;
    }
    void Update()
    {
        //pause the movement of the arrow once it has been fired so it doesnt continue to follow its direction
        if (psm.getPlayState() == PlayStateManager.PlayStates.Aiming)
        {
            //allows for the consitent back and fourth motion, once time.time * 0.85f > 1, resets back to 0 and so on 
            float rotation = Mathf.PingPong(Time.time * 0.85f, 1f);

            //allows the arrow to be locked between -60 and 60 degrees
            float rotationWindow = Mathf.Lerp(-60, 60, rotation);

            //only rotate in x is neccessary 
            transform.rotation = Quaternion.Euler(0f, rotationWindow, 0f);
        }
        else if (psm.getPlayState() == PlayStateManager.PlayStates.EnemyTurn)
        {
            float rotation = Mathf.PingPong(Time.time * 0.85f, 1f);
            float rotationWindow = Mathf.Lerp(-30, 30, rotation);
            transform.rotation = Quaternion.Euler(0f, rotationWindow, 0f);
        }
    }
}
