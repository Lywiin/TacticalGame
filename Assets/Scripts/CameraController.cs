using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float thrust = 200;
    public float minZ = -5, maxZ = 25;
    public float maxVelocity = 10;
    public InputController inputController;

    private Rigidbody rb;


    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {

        if (inputController.SwipeUpRight)
            rb.AddRelativeForce(transform.forward * thrust);
        if (inputController.SwipeDownLeft)
            rb.AddRelativeForce(transform.forward * thrust * -1);

        //Clamp max velocity
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, Mathf.Clamp(rb.velocity.z, -maxVelocity, maxVelocity));

        //Slow the camera down when there is no drag
        if (!inputController.IsDragging)
            rb.velocity = rb.velocity * 0.975f;

        //Clamp camera position
        transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Clamp(transform.position.z, minZ, maxZ));
    }
}
