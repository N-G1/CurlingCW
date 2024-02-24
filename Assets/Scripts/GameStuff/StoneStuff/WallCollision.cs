using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCollision : MonoBehaviour
{
    private GameLoop gl;

    void Start()
    {
        gl = GameLoop.GLInstance;   
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            //Vector3 direction = Vector3.Reflect(gl.getCurrVelocity(), collision.contacts[0].normal);
            //transform.position = collision.contacts[0].point + collision.contacts[0].normal * 0.1f;
            ////transform.GetChild(1).transform.forward = direction;
            gl.setCurrSpeed(0);
        }
    }
}
