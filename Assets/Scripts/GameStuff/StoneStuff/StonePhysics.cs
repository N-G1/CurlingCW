using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//script that attempts to emulate sliding on ice and the physics of the stones 
public class StonePhysics : MonoBehaviour
{ 
    private Rigidbody rb;
    private Collider col;

    private const float friction = 0.03f; //object friction
    private const float bounce = 0.95f; // allows for bouncing off walls / other stones
    private const float drag = 0f; 
    private const float angDrag = 0.01f; 

    // Assign custom material to each stone, set up physics stuff 
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Stone")
        {
            Rigidbody otherStone = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 direction = otherStone.transform.position - transform.position;
            rb.AddForce(direction.normalized * 1.5f, ForceMode.Impulse);
            otherStone.AddForce(-direction.normalized * 1.5f, ForceMode.Impulse);
        }
    }
}
