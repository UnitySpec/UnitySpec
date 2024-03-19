using Assets.Scripts;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float horizontalInput;
    private float forwardInput;
    private float speed = 20.0f;
    private float force = 10f;
    Rigidbody rb;


    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get inputs
        horizontalInput = InputAbstraction.GetAxis("Horizontal");
        forwardInput = InputAbstraction.GetAxis("Vertical");

        // Move player
        transform.Translate(Vector3.forward * Time.deltaTime * speed * forwardInput);
        //transform.Translate(Vector3.right * Time.deltaTime * speed * horizontalInput);
        rb.AddForce(Vector3.right * Time.deltaTime * force * horizontalInput, ForceMode.VelocityChange);
    }
}
