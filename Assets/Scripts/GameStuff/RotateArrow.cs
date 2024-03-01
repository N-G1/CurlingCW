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
        if (psm.GetPlayState() == PlayStateManager.PlayStates.Aiming)
        {
            //allows for the consitent back and fourth motion, once time.time * 0.85f > 1, resets back to 0 and so on 
            float rotation = Mathf.PingPong(Time.time * 0.85f, 1f);

            //allows the arrow to be locked between -60 and 60 degrees
            float rotationWindow = Mathf.Lerp(-60, 60, rotation);

            //only rotate in x is neccessary 
            transform.rotation = Quaternion.Euler(0f, rotationWindow, 0f);
        }
        else if (psm.GetPlayState() == PlayStateManager.PlayStates.EnemyTurn)
        {
            float rotation = Mathf.PingPong(Time.time * 0.85f, 1f);
            float rotationWindow = 0f;

            //higher difficulty = tighter potential slide angle
            switch (PlayerPrefs.GetInt("AIDifficulty"))
            {
                case (1):
                    rotationWindow = Mathf.Lerp(-45, 45, rotation);
                    break;
                case (2):
                    rotationWindow = Mathf.Lerp(-25, 25, rotation);
                    break;
                case (3):
                    rotationWindow = Mathf.Lerp(-18, 18, rotation);
                    break;
            }
                
            transform.rotation = Quaternion.Euler(0f, rotationWindow, 0f);
        }
    }
}
