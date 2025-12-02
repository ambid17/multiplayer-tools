using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

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
    Transform target;

    void Start()
    {
        target = transform.parent;
        lookAction = InputSystem.actions.FindAction("Look");
    }

    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        
        if(Camera.main == null)
        {
            Debug.LogWarning("No main camera found for CameraFollow script.");
            return;
        }

        if (IsOwner)
        {
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = Vector3.zero;
            Camera.main.transform.localRotation = Quaternion.identity;
        }
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
