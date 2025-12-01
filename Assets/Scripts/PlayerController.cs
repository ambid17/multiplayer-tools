using Digger.Modules.Core.Sources;
using Digger.Modules.Runtime.Sources;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
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
    [SerializeField] private float opacity = 0.5f;
    [SerializeField] private float brushSize = 0.5f;
    [SerializeField] private Vector3 lastFrameLocation;

    void Start()
    {
        digger = FindAnyObjectByType<DiggerMasterRuntime>();
        rb = GetComponent<Rigidbody>();
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (GameManager.Instance.IsPaused) return;
        moveInputs = moveAction.ReadValue<Vector2>();
        isJumping = jumpAction.ReadValue<float>() != 0 ? true : false;
        //isGrounded = Physics.Raycast(transform.position, Vector3.down, transform.localScale.x + 0.01f, groundLayer);

        velocity = rb.linearVelocity;
        velocityMagnitude = velocity.magnitude;

        var horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        if (horizontalVelocity.magnitude > 0.1f)
        {
            float scaleIncrease = growSpeed * horizontalVelocity.magnitude * Time.deltaTime;
            transform.localScale += new Vector3(scaleIncrease, scaleIncrease, scaleIncrease);
            rb.mass = transform.localScale.x;

            if (Physics.Raycast(transform.position, Vector3.down, out var hit, transform.localScale.y + 0.01f))
            {
                digger.Modify(hit.point, brushType, actionType, textureIndex, opacity, transform.localScale.x);
            }
        }

        lastFrameLocation = transform.position;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.IsPaused) return;
        Vector3 forceDirection = Vector3.zero;
        var movementRelativeToCamera = new Vector3(moveInputs.x, 0, moveInputs.y);
        var cameraLookingDirection = cameraTransform.rotation * Vector3.forward;
        cameraLookingDirection = new Vector3(cameraLookingDirection.x, 0f, cameraLookingDirection.z).normalized;
        var cameraRelativeForce = Quaternion.FromToRotation(Vector3.forward, cameraLookingDirection) * movementRelativeToCamera;

        rb.AddForce(cameraRelativeForce * moveSpeed * transform.localScale.x);
        if (isJumping && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == GameManager.GroundLayer)
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == GameManager.GroundLayer)
        {
            isGrounded = false;
        }
    }
}
