using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Rigidbody rb;
    public float speed = 10f;

    InputAction moveAction;
    InputAction jumpAction;
    Vector2 moveInputs;

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
        velocity = rb.linearVelocity;
    }

    void FixedUpdate()
    {
        rb.AddForce(new Vector3(moveInputs.x, 0, moveInputs.y) * speed);
    }
}
