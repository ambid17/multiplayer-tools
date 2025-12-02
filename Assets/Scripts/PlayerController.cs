using Digger.Modules.Core.Sources;
using Digger.Modules.Runtime.Sources;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    private Rigidbody rb;
    public float moveSpeed = 10f;
    public float growSpeed = 0.001f;
    public float jumpForce = 5f;
    public LayerMask groundLayer;
    public Transform cameraTransform;

    InputAction moveAction;
    InputAction jumpAction;
    Vector2 moveInputs;
    [Header("Debug info")]
    [SerializeField]
    bool isGrounded;
    [SerializeField]
    bool isJumping;

    [SerializeField]
    private Vector3 velocity;
    [SerializeField]
    private float velocityMagnitude;
    public static float theoreticalMaxVelocity = 15f;
    public static float minSpeedToGrow = 1f;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        Cursor.lockState = CursorLockMode.Locked;
    }

    public override void OnStartClient()
    {
        if (!IsOwner)
        {
            GetComponent<PlayerInput>().enabled = true;
        }
    }

    // TODO: follow this: https://www.youtube.com/watch?v=9aPBqiaV8fE
    void Update()
    {
        if (!IsOwner)
            return;
        if (GameManager.Instance.IsPaused) return;
        moveInputs = moveAction.ReadValue<Vector2>();
        isJumping = jumpAction.ReadValue<float>() != 0 ? true : false;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, transform.localScale.x + 0.01f, groundLayer);

        velocity = rb.linearVelocity;
        velocityMagnitude = velocity.magnitude;

        var horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        if (horizontalVelocity.magnitude > minSpeedToGrow)
        {
            // TODO: lower in size if the ground isn't snow, and dont deform the terrain
            float scaleIncrease = growSpeed * horizontalVelocity.magnitude * Time.deltaTime;
            transform.localScale += new Vector3(scaleIncrease, scaleIncrease, scaleIncrease);
            rb.mass = transform.localScale.x;
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner)
            return;
        if (GameManager.Instance.IsPaused) return;
        Vector3 forceDirection = Vector3.zero;
        var movementRelativeToCamera = new Vector3(moveInputs.x, 0, moveInputs.y);
        var cameraLookingDirection = cameraTransform.rotation * Vector3.forward;
        cameraLookingDirection = new Vector3(cameraLookingDirection.x, 0f, cameraLookingDirection.z).normalized;
        var cameraRelativeForce = Quaternion.FromToRotation(Vector3.forward, cameraLookingDirection) * movementRelativeToCamera;

        rb.AddForce(cameraRelativeForce * moveSpeed * rb.mass);
        if (isJumping && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }
}

public class DigAction
{
    public Vector3 position;
    public float size;
    public float opacity;
}
