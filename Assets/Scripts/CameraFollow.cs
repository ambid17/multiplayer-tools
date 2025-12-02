using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

// TODO: make this not a child of the player, otherwise jitter
public class CameraFollow : NetworkBehaviour
{
    [Header("Settings")]
    public float distance = 4f;
    public float distanceGrowth = 1f;

    public float mouseSensitivity = 50f;
    public float verticalMin = -30f;
    public float verticalMax = 70f;

    public float rotationSmoothTime = 0.12f;
    Vector3 rotationSmoothVelocity;

    InputAction lookAction;
    float yaw;
    float pitch;
    Vector3 currentRotation;
    public Transform target;

    void Start()
    {
        lookAction = InputSystem.actions.FindAction("Look");
    }

    void LateUpdate()
    {
        if(GameManager.Instance.IsPaused) return;
        if (!target) return;
        Vector2 lookInputs = lookAction.ReadValue<Vector2>();
        yaw += lookInputs.x * mouseSensitivity * Time.deltaTime;
        pitch -= lookInputs.y * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, verticalMin, verticalMax);

        Vector3 targetRotation = new Vector3(pitch, yaw);
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationSmoothVelocity, rotationSmoothTime);

        transform.eulerAngles = currentRotation;

        // --- Camera collision ---
        float desiredDistance = distance + target.transform.localScale.x * distanceGrowth;
        


        // Move camera
        transform.position = target.position - transform.forward * desiredDistance;
    }
}
