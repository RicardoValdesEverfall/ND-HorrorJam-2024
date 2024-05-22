using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 5f; // The speed of camera rotation
    public float minYAngle = -80f; // Minimum y-axis angle
    public float maxYAngle = 80f; // Maximum y-axis angle
    public float smoothSpeed = 5f; // The speed of smoothing
    public float breathingStrength = 0.05f; // The strength of breathing effect
    public float breathingSpeed = 1f; // The speed of breathing effect
    public float normalFOV = 60f; // Normal FOV
    public float maxZoomFOV = 30f; // Maximum FOV for zoom
    public float zoomSpeed = 2f; // The speed of camera zoom

    private float rotationX = 0f; // Current rotation around the x-axis
    private float currentRotationY = 0f; // Accumulated horizontal rotation
    private Vector3 originalPosition; // Original position of the camera
    private Camera mainCamera;
    private bool isZooming = false; // Flag to track if currently zooming

    void Start()
    {
        originalPosition = transform.localPosition;
        mainCamera = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Get mouse input for camera rotation
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Rotate the camera based on mouse movement
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, minYAngle, maxYAngle);

        // Accumulate horizontal rotation
        currentRotationY += mouseX;
        currentRotationY = Mathf.Clamp(currentRotationY, minYAngle, maxYAngle);

        Quaternion targetRotation = Quaternion.Euler(rotationX, currentRotationY, 0);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, smoothSpeed * Time.deltaTime);

        // Apply breathing effect
        float breathingOffset = Mathf.Sin(Time.time * breathingSpeed) * breathingStrength;
        Vector3 breathingPosition = originalPosition + new Vector3(0, breathingOffset, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, breathingPosition, smoothSpeed * Time.deltaTime);

        // Handle camera zoom using FOV
        if (Input.GetMouseButtonDown(1)) // Right mouse button pressed
        {
            isZooming = true;
        }

        if (Input.GetMouseButtonUp(1)) // Right mouse button released
        {
            isZooming = false;
        }

        if (isZooming)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, maxZoomFOV, smoothSpeed * Time.deltaTime);
        }
        else
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, normalFOV, smoothSpeed * Time.deltaTime);
        }
    }
}



/**public class CameraController : MonoBehaviour
{ 
    public float rotationSpeed = 5f; // The speed of camera rotation
    public float minYAngle = -80f; // Minimum y-axis angle
    public float maxYAngle = 80f; // Maximum y-axis angle
    public float smoothSpeed = 5f; // The speed of smoothing
    public float breathingStrength = 0.05f; // The strength of breathing effect
    public float breathingSpeed = 1f; // The speed of breathing effect

    private float rotationX = 0f; // Current rotation around the x-axis
    private float currentRotationY = 0f; // Accumulated horizontal rotation
    private Vector3 originalPosition; // Original position of the camera

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        // Get mouse input for camera rotation
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Rotate the camera based on mouse movement
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, minYAngle, maxYAngle);

        // Accumulate horizontal rotation
        currentRotationY += mouseX;

        Quaternion targetRotation = Quaternion.Euler(rotationX, currentRotationY, 0);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, smoothSpeed * Time.deltaTime);

        // Apply breathing effect
        float breathingOffset = Mathf.Sin(Time.time * breathingSpeed) * breathingStrength;
        Vector3 breathingPosition = originalPosition + new Vector3(0, breathingOffset, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, breathingPosition, smoothSpeed * Time.deltaTime);
    }
}**/
