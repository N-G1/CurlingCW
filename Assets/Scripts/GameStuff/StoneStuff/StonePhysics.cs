using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//script that attempts to emulate sliding on ice and the physics of the stones 
public class StonePhysics : MonoBehaviour
{ 
    private Rigidbody rb;
    private Collider col;
    // Assign custom material to each stone
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        col.material = new PhysicMaterial
        {
            dynamicFriction = 0.03f,
            staticFriction = 0.03f,
            bounciness = 0.75f,   
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
