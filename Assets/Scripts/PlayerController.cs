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
    public DiggerMasterRuntime digger;

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

    [SerializeField] private BrushType brushType = BrushType.Sphere;
    [SerializeField] private ActionType actionType = ActionType.Dig;
    [SerializeField] private int textureIndex = 0;
    [SerializeField] private float opacityMax = 0.5f;
    [SerializeField] private float minSpeedToPaint = 1f;
    [SerializeField] private int frameInPastToPaint = 5;
    [SerializeField] private float digSizeMultiplier = 0.8f;
    private float theoreticalMaxVelocity = 15f;
    private List<DigAction> digActions = new List<DigAction>();

    void Start()
    {
        digger = FindAnyObjectByType<DiggerMasterRuntime>();
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
    // move digging to its own script
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
        if (horizontalVelocity.magnitude > minSpeedToPaint)
        {
            // TODO: lower in size if the ground isn't snow, and dont deform the terrain
            float scaleIncrease = growSpeed * horizontalVelocity.magnitude * Time.deltaTime;
            transform.localScale += new Vector3(scaleIncrease, scaleIncrease, scaleIncrease);
            rb.mass = transform.localScale.x;

            var opacity = Mathf.Lerp(0, opacityMax, horizontalVelocity.magnitude / theoreticalMaxVelocity);
            var digLocation = transform.position - (velocity * Time.deltaTime * frameInPastToPaint);
            var digSize = transform.localScale.x * digSizeMultiplier;
            if (Physics.Raycast(digLocation, Vector3.down, out var hit, transform.localScale.y + 0.01f, groundLayer))
            {
                digger.ModifyAsyncBuffured(digLocation, brushType, actionType, textureIndex, opacity, digSize);
                digActions.Add(new DigAction
                {
                    position = digLocation,
                    size = digSize,
                    opacity = opacity
                });
            }
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
