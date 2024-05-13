using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLights : MonoBehaviour
{
    private Quaternion newRot;
    public float speed;
    void Start()
    {
        newRot = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion currentRotation = transform.rotation;

        // Convert the rotation to euler angles
        Vector3 currentEulerAngles = currentRotation.eulerAngles;

        // Modify only the Z-axis rotation
        float newZRotation = currentEulerAngles.z + speed * Time.deltaTime;

        // Create a new rotation with modified Z-axis rotation
        Quaternion newRotation = Quaternion.Euler(currentEulerAngles.x, currentEulerAngles.y, newZRotation);

        // Apply the new rotation to the object
        transform.rotation = newRotation;
    }
}
