using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// script that attempts to emulate sliding on ice and the physics of the stones 
/// used with AI stones as they use rb, only the player uses manual physics 
/// </summary>
public class AIStonePhysics : MonoBehaviour
{ 
    private Rigidbody rb;
    private Collider col;

    private Vector3 originalPosition;
    private bool collided = false;

    private const float friction = 0.03f; //object friction
    private const float bounce = 0.95f; // allows for bouncing off walls / other stones
    private const float drag = 0f; 
    private const float angDrag = 0.01f;

    /// <summary>
    /// Assign custom material to each stone, set up physics stuff (AI stones)
    /// </summary>
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.angularDrag = angDrag;
        rb.drag = drag;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; //continous for less inaccurate collisions at high speeds 
        rb.freezeRotation = true;

        col.material = new PhysicMaterial
        {
            dynamicFriction = friction,
            staticFriction = friction,
            bounciness = bounce, 
        };
    }

    /// <summary>
    /// Prevents several bugs involving the original stones position when colliding with more than 1 player stone
    /// </summary>
    void Update()
    {
        if (collided)
        {
            transform.position = new Vector3 (originalPosition.x, 0.5f, originalPosition.z);
        }
    }

    /// <summary>
    /// Handles collisions
    /// </summary>
    /// <param name="collision">detected collision</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Stone" && collision.gameObject.layer == 8)
        {
            originalPosition = transform.position;
            collided = true;
            StartCoroutine(handlePlayerEnemyCollision(collision));
        }
        else if (collision.gameObject.tag == "Stone" && collision.gameObject.layer == 9)
        {
            Rigidbody otherStone = collision.gameObject.GetComponent<Rigidbody>();
            
            Vector3 direction = otherStone.transform.position - transform.position;
            //rb.AddForce(direction.normalized * 1.5f, ForceMode.Impulse);
            otherStone.AddForce(-direction.normalized * 1.5f, ForceMode.Impulse);
        }
    }
    /// <summary>
    /// Handles the direction to push the player stone in 
    /// </summary>
    /// <param name="playerStone">stone to be pushed</param>
    /// <returns></returns>
    private IEnumerator handlePlayerEnemyCollision(Collision playerStone)
    {
        //gets the direction of the collision by taking the difference of the point of collision and the location of the stone 
        Vector3 colDir = (playerStone.contacts[0].point - playerStone.transform.position).normalized;
        //exclude Y axis as was causing bugs and not needed anyway
        colDir = new Vector3(colDir.x, 0f, colDir.z);
        float collisionMultiplier = 10f;
        Vector3 prevPos, nextPos;

        while (collisionMultiplier > 0.1f)
        {
            prevPos = playerStone.transform.position;
            nextPos = prevPos + -(colDir) * collisionMultiplier * Time.deltaTime;
            playerStone.transform.position = nextPos;
            collisionMultiplier *= 0.95f;
            yield return null;
        }

        //unfreeze stones position and set its velocity to 0 
        rb.velocity = new Vector3(0f, 0f, 0f);
        yield return new WaitForSeconds(0.5f); 
        collided = false;
    }
}
