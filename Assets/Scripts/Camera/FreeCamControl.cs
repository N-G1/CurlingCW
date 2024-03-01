using UnityEngine;

public class FreeCamControl : MonoBehaviour
{
    private float horizInput, vertInput;
    private Vector3 movDirection;
    private Rigidbody rb;

    [SerializeField] private Transform camOrientation;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        horizInput = Input.GetAxisRaw("Horizontal");
        vertInput = Input.GetAxisRaw("Vertical");    
    }

    //FixedUpdate for rb and physics stuff 
    void FixedUpdate()
    {
        //take into account the orientation of the camera before calculating the direction to move
        //otherwise wasd will always travel in the same direction regardless of where you are looking
        movDirection = camOrientation.forward * vertInput + camOrientation.right * horizInput;
        
        //cameras on rbs can be choppy so lerp to smooth 
        rb.velocity = Vector3.Lerp(rb.velocity, movDirection.normalized * 10f, Time.fixedDeltaTime * 5f);
    }
}
