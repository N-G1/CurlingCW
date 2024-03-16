using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles bouncing off walls and collisions with other stones. AI stones only use their rb's for 
/// AI on AI collisions, any other form goes through custom collision detection
/// </summary>
public class CollisionHandler : MonoBehaviour
{
    private GameLoop gl;
    private Rigidbody rb;
    private Vector3 originalPosition;
    private bool collided = false, AIStone = false, bounced = false;
    private float collisionMultiplier;
    private Vector3 currentVelocity;

    [SerializeField] LayerMask playerLayer, wallLayer, enemyLayer;

    void Start()
    {
        gl = GameLoop.GLInstance;
    }

    /// <summary>
    /// Freezes the position of the stone that collided temporarily as I felt it better simulated the 
    /// transfer of momentum 
    /// </summary>
    void Update()
    {      
        if (collided)
        {
            gl.SetCurrSpeed(0f);
            if (AIStone)
            {
                rb.velocity = new Vector3(0f, 0f, 0f);
            }
            transform.position = originalPosition;
        }
        CastColliderRays();
    }

    /// <summary>
    /// Starts the coroutine that handles wall collision in the event it makes contact with a wall 
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") && gameObject.layer == 8)
        {
            StartCoroutine(BounceAdjustment(collision));
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
        if (Physics.Raycast(transform.GetChild(1).position, direction.normalized, out RaycastHit hit, 0.325f, playerLayer | enemyLayer, QueryTriggerInteraction.Ignore) && !bounced && !hit.collider.CompareTag("CurrStone"))
        {
            if (rb)
            {
                AIStone = true;
            }
            StartCoroutine(HandlePlayerStoneCollision(hit.collider, hit.point));
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

        //different multiplier due to rb
        if (AIStone)
        {
            collisionMultiplier = gl.GetCurrSpeed() > 5.25 ? 1f : gl.GetCurrSpeed() > 3 ? 1.2f : 1.4f;
        }
        else
        {
            collisionMultiplier = gl.GetCurrSpeed() > 5.25 ? 0.5f : gl.GetCurrSpeed() > 3 ? 1.25f : 1.5f;
        }
        collided = true;

        gl.SetCollided(true);

        //applies force to stone and gradually slows it to simulate it being knocked 
        while (collisionMultiplier > 0.1f)
        {
            prevPos = otherPStone.transform.position;
            nextPos = prevPos + -(colDir) * collisionMultiplier * Time.deltaTime;
            otherPStone.transform.position = Vector3.Lerp(nextPos, prevPos, 0.25f);
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
    private IEnumerator BounceAdjustment(Collision wallCol)
    {
        float newSpeed;
        Vector3 prevPos, newPos;
        gl.SetBouncedOffWall(true);

        bounced = true;
        Vector3 direction;
        Vector3 originalDir;

        //make the new direction the contact point's normal 
        if (gameObject.CompareTag("CurrStone"))
        {
            direction = Vector3.Reflect(gl.GetCurrVelocity(), wallCol.contacts[0].normal);
        }
        else
        {
            direction = Vector3.Reflect(currentVelocity, wallCol.contacts[0].normal);
        }


        //as centre child determines travelling direction, make the forward vector this new direction (y axis is excluded as not relevant)
        if (direction != Vector3.zero)
        {
            transform.GetChild(1).forward = new Vector3(direction.x, 0f, direction.z);
        }
        originalDir = transform.GetChild(1).forward;
        newSpeed = gl.GetCurrSpeed() > 6 ? 0.1f : gl.GetCurrSpeed() > 4 ? 0.15f : gl.GetCurrSpeed() > 1 ? 0.25f : 0.5f;

        //slow the object to a stop 
        while (newSpeed >= 0.01f)
        {
            prevPos = transform.position;
            newPos = prevPos + direction * (gl.GetCurrSpeed() * newSpeed) * Time.fixedDeltaTime;

            newSpeed *= 0.99f;
            //transform.position = !atBoundry() ? newPos : new Vector3(wallCol.contacts[0].point.x + wallCol.contacts[0].normal.x * 0.01f, 0.5f, wallCol.contacts[0].point.z + wallCol.contacts[0].normal.z - 1f);

            //atBoundry attempts to prevent stone glitching into wall
            if (atBoundry())
            {
                gl.SetCurrSpeed(0f);
                collisionMultiplier = 0f;
                transform.GetChild(1).forward = originalDir;
                transform.position = new Vector3(wallCol.contacts[0].point.x + wallCol.contacts[0].normal.x * 0.1f, 0.5f, wallCol.contacts[0].point.z + wallCol.contacts[0].normal.z - 1f);
                yield break;
            }
            else
            {
                transform.position = newPos;
                gl.SetCurrSpeed((newPos - prevPos).magnitude / Time.fixedDeltaTime);
            }

            yield return null;
        }
        transform.GetChild(1).forward = originalDir;
        bounced = false;
    }

    private bool atBoundry()
    {
        if (transform.position.x > 5.8 || transform.position.x < -0.8 || transform.position.z < -2.8)
        {
            return true;
        }
        return false;
    }
}