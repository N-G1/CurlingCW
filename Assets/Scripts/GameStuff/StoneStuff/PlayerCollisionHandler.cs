using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    private GameLoop gl;
    private Vector3 originalPosition;
    private bool collided = false;

    [SerializeField] LayerMask playerLayer;

    void Start()
    {
        gl = GameLoop.GLInstance;   
    }

    void Update()
    {
        castColliderRays();
        if (collided)
        {
            transform.position = new Vector3(originalPosition.x, 0.5f, originalPosition.z);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            StartCoroutine(bounceAdjustment(collision)); 
        }
    }

    /// <summary>
    /// Raycasts out from centre of stone to check if any other stones are within range
    /// </summary>
    private void castColliderRays()  
    {
        //Not sure if this is the best way to do this but it seems Unitys collision detection without
        //an rb is extremely limited, primarity selected this as raycastHit will contain contact point information
        Vector3[] directionsToCast = { transform.forward, transform.right, -(transform.forward), -(transform.right) };
        for (int i = 0; i < directionsToCast.Length; i++)
        {
            Vector3 nextDir;
            Vector3 centreDir;
            Vector3 currDir = directionsToCast[i];
            if (i != directionsToCast.Length - 1)
            {
                nextDir = directionsToCast[i + 1];
                centreDir = Vector3.Slerp(currDir, nextDir, 0.5f);
            }
            else
            {
                nextDir = directionsToCast[0];
                centreDir = Vector3.Slerp(currDir, nextDir, 0.5f);
            }

            castRay(currDir);
            castRay(nextDir);
            castRay(centreDir);
        }
    }

    /// <summary>
    /// Casts ray out from passed angle
    /// </summary>
    /// <param name="direction">direction to cast</param>
    private void castRay(Vector3 direction)
    {
        RaycastHit hit;
                                                        //0.65 is the size of the object in x and z
        //if (Physics.Raycast(new Ray(transform.GetChild(1).position, direction.normalized * 0.65f), out hit, playerLayer))
        //{
        //    collided = true;
        //    StartCoroutine(handlePlayer2PlayerCollision(hit.collider, hit.point));            fix 
        //}
    }
    
    /// <summary>
    /// Handles what to do when a player stone collides with another player stone
    /// </summary>
    /// <param name="otherPStone">collider of other player stone</param>
    /// <param name="contactPoint">contact point of other collider</param>
    /// <returns></returns>
    private IEnumerator handlePlayer2PlayerCollision(Collider otherPStone, Vector3 contactPoint)
    {
        //prevents more than 1 contact point being used
        if (!collided)
        {
            Vector3 prevPos, nextPos;
            //gets the direction of the collision by taking the difference of the point of collision and the location of the stone 
            Vector3 colDir = (contactPoint - otherPStone.transform.position).normalized;
            //exclude Y axis as was causing bugs and not needed anyway
            colDir = new Vector3(colDir.x, 0f, colDir.z);
            float collisionMultiplier = 10f;
            collided = true;
            gl.setCurrSpeed(0f);

            while (collisionMultiplier > 0.1f)
            {
                prevPos = otherPStone.transform.position;
                nextPos = prevPos + -(colDir) * collisionMultiplier * Time.deltaTime;
                otherPStone.transform.position = nextPos;
                collisionMultiplier *= 0.95f;
                yield return null;
            }

            //unfreeze stones position 
            yield return new WaitForSeconds(0.5f);
            collided = false;
        }
        
    }

    /// <summary>
    /// Adjusts the direction and the speed if stone collides with a wall
    /// Coroutine same as handlePhysics() as using main thread resulted in choppy movement
    /// </summary>
    /// <param name="col">collider</param>
    /// <returns></returns>
    private IEnumerator bounceAdjustment(Collision wallCol)
    {
        float newSpeed;
        Vector3 prevPos, newPos;
        gl.setBouncedOffWall(true);

        //make the new direction the contact point's normal 
        Vector3 direction = Vector3.Reflect(gl.getCurrVelocity(), wallCol.contacts[0].normal);
        //as centre child determines travelling direction, make the forward vector this new direction (y axis is excluded as not relevant)
        transform.GetChild(1).forward = new Vector3(direction.x, 0f, direction.z);

        newSpeed = gl.getCurrSpeed() > 3 ? 0.8f : gl.getCurrSpeed() > 2 ? 1f : gl.getCurrSpeed() > 1 ? 1.2f : 1.4f;

        //slow the object to a stop 
        while (newSpeed >= 0.01f)
        {
            prevPos = transform.position;
            newPos = prevPos + direction * (gl.getCurrSpeed() * newSpeed) * Time.deltaTime;
            transform.position = newPos;
            newSpeed *= 0.99f;
            gl.setCurrSpeed((newPos - prevPos).magnitude / Time.fixedDeltaTime);
            yield return null;
        }
    }
}
