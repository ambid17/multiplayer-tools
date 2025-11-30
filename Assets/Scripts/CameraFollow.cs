using FishNet.Transporting.Yak;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Settings")]
    public float distance = 4f;
    public float minDistance = 2f;
    public float maxDistance = 6f;

    public float mouseSensitivity = 180f;
    public float verticalMin = -30f;
    public float verticalMax = 70f;

    public float rotationSmoothTime = 0.12f;
    Vector3 rotationSmoothVelocity;

    InputAction lookAction;
    float yaw;
    float pitch;
    Vector3 currentRotation;

    void Start()
    {
        lookAction = InputSystem.actions.FindAction("Look");
    }

    void LateUpdate()
    {
        if (!target) return;
        Vector2 lookInputs = lookAction.ReadValue<Vector2>();
        yaw += lookInputs.x * mouseSensitivity * Time.deltaTime;
        pitch -= lookInputs.y * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, verticalMin, verticalMax);

        Vector3 targetRotation = new Vector3(pitch, yaw);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationSmoothVelocity, rotationSmoothTime);

        transform.eulerAngles = currentRotation;

        // --- Camera collision ---
        float desiredDistance = distance;
        Ray ray = new Ray(target.position, -transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, distance))
        {
            desiredDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
        }

        // Move camera
        transform.position = target.position - transform.forward * desiredDistance;
    }
}
