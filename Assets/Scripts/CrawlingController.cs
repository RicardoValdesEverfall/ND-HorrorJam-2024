using UnityEngine;

public class CrawlingController : MonoBehaviour
{
    public float moveSpeed = 1.0f; // Speed of crawling
    public float rotationSpeed = 30.0f; // Speed of rotation
    public float bobFrequency = 2.0f; // Frequency of the bobbing effect
    public float bobAmplitude = 0.1f; // Amplitude of the bobbing effect

    private float bobOffset;
    private Rigidbody rb;
    private float initialY;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialY = transform.position.y;
        bobOffset = 0.0f;
    }

    void Update()
    {
        HandleMovement();
        HandleBobbing();
    }

    void HandleMovement()
    {
        // Handle forward and backward movement
        float moveDirection = Input.GetAxis("Vertical");
        Vector3 movement = transform.forward * moveDirection * moveSpeed * Time.deltaTime;

        // Apply movement
        rb.MovePosition(rb.position + movement);

        // Handle rotation
        float rotation = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;
        Quaternion turn = Quaternion.Euler(0f, rotation, 0f);

        // Apply rotation
        rb.MoveRotation(rb.rotation * turn);
    }

    void HandleBobbing()
    {
        // Calculate the bobbing effect
        bobOffset += bobFrequency * Time.deltaTime;
        float bobbing = Mathf.Sin(bobOffset) * bobAmplitude;

        // Apply the bobbing effect to the character's y position
        Vector3 newPosition = transform.position;
        newPosition.y = initialY + bobbing;
        rb.MovePosition(newPosition);
    }
}

