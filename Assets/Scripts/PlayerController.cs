using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Rigidbody rb;
    public float moveSpeed = 10f;
    public float growSpeed = 0.001f;
    public float jumpForce = 5f;
    public LayerMask groundLayer;
    public Transform cameraTransform;

    private static readonly int GroundLayer = 6;

    InputAction moveAction;
    InputAction jumpAction;
    Vector2 moveInputs;
    [SerializeField]
    bool isGrounded;
    [SerializeField]
    bool isJumping;

    [SerializeField]
    private Vector3 velocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    void Update()
    {
        moveInputs = moveAction.ReadValue<Vector2>();
        isJumping = jumpAction.ReadValue<float>() != 0 ? true : false;
        //isGrounded = Physics.Raycast(transform.position, Vector3.down, transform.localScale.x + 0.01f, groundLayer);
        Debug.DrawRay(transform.position, Vector3.down * (transform.localScale.x + 0.01f), Color.red);

        velocity = rb.linearVelocity;
        if(velocity.magnitude > 0.1f)
        {
            float scaleIncrease = growSpeed * velocity.magnitude * Time.deltaTime;
            transform.localScale += new Vector3(scaleIncrease, scaleIncrease, scaleIncrease);
        }
    }

    void FixedUpdate()
    {
        Vector3 forceDirection = Vector3.zero;
        var movementRelativeToCamera = new Vector3(moveInputs.x, 0, moveInputs.y);
        var cameraLookingDirection = cameraTransform.rotation * Vector3.forward;
        cameraLookingDirection = new Vector3(cameraLookingDirection.x, 0f, cameraLookingDirection.z).normalized;
        var cameraRelativeForce = Quaternion.FromToRotation(Vector3.forward, cameraLookingDirection) * movementRelativeToCamera;

        rb.AddForce(cameraRelativeForce * moveSpeed);
        if (isJumping && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == GroundLayer)
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == GroundLayer)
        {
            isGrounded = false;
        }
    }
}
