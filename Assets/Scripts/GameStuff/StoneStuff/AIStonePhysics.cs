using UnityEngine;


/// <summary>
/// Sets up AI stone materials and handles collisions with AI stones 
/// </summary>
public class AIStonePhysics : MonoBehaviour
{ 
    private Rigidbody rb;
    private Collider col;

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
    /// Handles collisions with other AI stones
    /// </summary>
    /// <param name="collision">detected collision</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Stone") && collision.gameObject.layer == 9)
        {
            Rigidbody otherStone = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 direction = otherStone.transform.position - transform.position;
            otherStone.AddForce(-direction.normalized * 1.5f, ForceMode.Impulse);
        }
    }
}
