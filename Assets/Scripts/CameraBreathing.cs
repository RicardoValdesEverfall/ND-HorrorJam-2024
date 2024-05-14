using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBreathing : MonoBehaviour
{
    public float maxRotation = 1.0f; // Maximum rotation angle for breathing
    public float minRotation = -.80f; // Minimum rotation angle for breathing
    public float breathSpeed = 0.65f; // Speed of the breathing effect

    private Quaternion originalRotation;

    void Start()
    {
        originalRotation = transform.rotation; // Store the original rotation of the camera
    }

    void Update()
    {
        // Calculate the new rotation based on a sinusoidal function for each axis
        float newXRotation = Mathf.Lerp(minRotation, maxRotation, (Mathf.Sin(Time.time * breathSpeed) + 1) / 2);
        float newYRotation = Mathf.Lerp(minRotation, maxRotation, (Mathf.Sin(Time.time * breathSpeed * 1.1f) + 1) / 2);
        float newZRotation = Mathf.Lerp(minRotation, maxRotation, (Mathf.Sin(Time.time * breathSpeed * 1.2f) + 1) / 2);

        // Create a new rotation quaternion based on the calculated values
        Quaternion newRotation = originalRotation * Quaternion.Euler(newXRotation, newYRotation, newZRotation);

        // Apply the new rotation to the camera
        transform.rotation = newRotation;
    }
}
