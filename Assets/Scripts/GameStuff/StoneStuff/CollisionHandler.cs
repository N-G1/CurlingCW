using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles bouncing off walls and collisions with other stones. Even though AI stones contain a rb they do not use it for 
/// the collisions, as I ironically found that the raycasting collison I used actually worked better (most of the time)
/// </summary>
public class CollisionHandler : MonoBehaviour
{
    private GameLoop gl;
    private Rigidbody rb;
    private Vector3 originalPosition;
    private bool collided = false, AIStone = false, bounced = false, wallNear = false;

    [SerializeField] LayerMask playerLayer, wallLayer, enemyLayer;

    void Start()
    {
        gl = GameLoop.GLInstance;
    }

    /// <summary>
    /// Freezes the position of the stone that collided temporarily as I felt it better simulated the 
    /// transferral of momentum 
    /// </summary>
    void Update()
    {
        CastColliderRays();
        if (collided || wallNear)
        {
            gl.setCurrSpeed(0f);
            if (AIStone)
            {
                rb.velocity = new Vector3(0f, 0f, 0f);
            }
            transform.position = new Vector3(originalPosition.x, 0.5f, originalPosition.z);
        }
    }

    /// <summary>
    /// Starts the coroutine that handles wall collision in the event it makes contact with a wall 
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") && gameObject.layer == 8)
        {
            Debug.Log("Bounced off surface");
            StartCoroutine(bounceAdjustment(collision)); 
        }
    }

    /// <summary>
    /// Raycasts out from centre of stone in several directions to check if any other stones are within collision range
    /// </summary>
    private void CastColliderRays()  
    {
        //selected raycasts as hit will contain point of contact information 
        Vector3[] directionsToCast = { transform.forward, transform.right, -(transform.forward), -(transform.right) };
        for (int i = 0; i < directionsToCast.Length; i++)
        {
            Vector3 nextDir;
            Vector3 currDir = directionsToCast[i];
            if (i != directionsToCast.Length - 1)
            {
                nextDir = directionsToCast[i + 1];
            }
            else
            {
                nextDir = directionsToCast[0];
            }
            CastRay(currDir);
            HandleCentralRays(currDir, nextDir);
            CastRay(nextDir);
        }
    }

    /// <summary>
    /// Slerp allows for uniformly drawn extra rays between the curr direction and the next 
    /// </summary>
    /// <param name="currDir">current directional vector</param>
    /// <param name="nextDir">next directional vector</param>
    private void HandleCentralRays(Vector3 currDir, Vector3 nextDir)
    {
        for (int i = 1; i < 5; i++)
        {
            CastRay(Vector3.Slerp(currDir, nextDir, i * 0.2f));
        }
    }
    /// <summary>
    /// Casts ray out in passed direction, ignoring own game object and looking for other player stones 
    /// </summary>
    /// <param name="direction">direction to cast</param>
    private void CastRay(Vector3 direction)
    {
        rb = gameObject.GetComponent<Rigidbody>();
        originalPosition = transform.position;
                                                    //0.325 is the size of the object in x and z
        Debug.DrawRay(transform.GetChild(1).position, direction.normalized * 0.325f);
        if (Physics.Raycast(transform.GetChild(1).position, direction.normalized, out RaycastHit hit, 0.325f, playerLayer, QueryTriggerInteraction.Ignore) && !bounced)
        {
            Debug.Log("Collision between player stones");
           
            if (rb)
            {
                AIStone = true;
            }
            
            StartCoroutine(HandlePlayerStoneCollision(hit.collider, hit.point));
        }
        //if raycast into wall | attempt to prevent passing through walls due to high speed collisions 
        else if (Physics.Raycast(transform.GetChild(1).position, direction.normalized, 0.325f, wallLayer, QueryTriggerInteraction.Ignore))
        {
            wallNear = true;
             if (rb)
            {
                rb.velocity = new Vector3(0f, 0f, 0f);
            }
        }
        //if raycast hits nothing, then wall cant be close, called so frequently that wallNear will be reset quickly anyway
        else
        {
            wallNear = false;
        }
    }
    
    /// <summary>
    /// Handles what to do when a player stone collides with another player stone
    /// </summary>
    /// <param name="otherPStone">collider of other player stone</param>
    /// <param name="contactPoint">contact point of other collider</param>
    /// <returns></returns>
    private IEnumerator HandlePlayerStoneCollision(Collider otherPStone, Vector3 contactPoint)
    {
        Vector3 prevPos, nextPos;
        //gets the direction of the collision by taking the difference of the point of collision and the location of the stone 
        Vector3 colDir = (contactPoint - otherPStone.transform.position).normalized;
        //exclude Y axis as was causing bugs and not needed anyway
        colDir = new Vector3(colDir.x, 0f, colDir.z);
        float collisionMultiplier;

        //due to the rb, need lower collision multiplier 
        if (AIStone)
        {
            collisionMultiplier = gl.getCurrSpeed() > 3.25 ? 2.5f : gl.getCurrSpeed() > 2 ? 1.5f : 1f;
        }
        else
        {
            collisionMultiplier = gl.getCurrSpeed() > 3.25 ? 4f : gl.getCurrSpeed() > 2 ? 2f : 1f;
        }
        collided = true;
        
        //applies force to stone and gradually slows it to simulate it being knocked 
        while (collisionMultiplier > 0.1f)
        {
            prevPos = otherPStone.transform.position;
            nextPos = prevPos + -(colDir) * collisionMultiplier * Time.deltaTime;
            otherPStone.transform.position = Vector3.Slerp(nextPos, prevPos, 0.25f);
            collisionMultiplier *= 0.95f;
            yield return null;
        }

        //unfreeze stones position 
        yield return new WaitForSeconds(0.5f);
        collided = false;
        AIStone = false;
    }

    /// <summary>
    /// Adjusts the direction and the speed if stone collides with a wall
    /// Coroutine same as handlePhysics() and above method as using main thread resulted in choppy movement
    /// </summary>
    /// <param name="col">collider</param>
    /// <returns></returns>
    private IEnumerator bounceAdjustment(Collision wallCol)
    {
        float newSpeed;
        Vector3 prevPos, newPos;
        gl.setBouncedOffWall(true);
        bounced = true;

        //make the new direction the contact point's normal 
        Vector3 direction = Vector3.Reflect(gl.getCurrVelocity(), wallCol.contacts[0].normal);
        //as centre child determines travelling direction, make the forward vector this new direction (y axis is excluded as not relevant)
        transform.GetChild(1).forward = new Vector3(direction.x, 0f, direction.z);

        newSpeed = gl.getCurrSpeed() > 3 ? 0.15f : gl.getCurrSpeed() > 2 ? 0.25f : gl.getCurrSpeed() > 1 ? 0.5f : 1f;

        //slow the object to a stop 
        while (newSpeed >= 0.01f)
        {
            prevPos = transform.position;
            newPos = prevPos + direction * (gl.getCurrSpeed() * newSpeed) * Time.fixedDeltaTime;
            transform.position = newPos;
            newSpeed *= 0.99f;
            gl.setCurrSpeed((newPos - prevPos).magnitude / Time.fixedDeltaTime);
            yield return null;
        }
        bounced = false;
    }
}
