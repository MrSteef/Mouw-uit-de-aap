using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float forceMultiplier = 10f;
    [SerializeField] private Transform orientation;
    [SerializeField] private float groundDrag;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    bool canJump = true;


    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;


    [Header("Ground check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundLayer;
    bool isGrounded;

    private Vector2 moveInput;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void OnEnable()
    {
        moveAction?.action.Enable();
    }

    private void OnDisable()
    {
        moveAction?.action.Disable();
    }

    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + groundCheckDistance, groundLayer);

        if (moveAction == null)
        {
            moveInput = Vector2.zero;
        }
        else
        {
            moveInput = moveAction.action.ReadValue<Vector2>();
        }

        if (isGrounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }

        if (jumpAction.action.IsPressed() && canJump && isGrounded)
        {
            canJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        SpeedControl();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        if (orientation == null)
            return;

        Vector3 moveDirection =
            orientation.forward * moveInput.y +
            orientation.right * moveInput.x;

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        if (isGrounded)
        {

            rb.AddForce(
                moveDirection * moveSpeed * forceMultiplier,
                ForceMode.Force
            );
        }
        else
        {
            rb.AddForce(
                moveDirection * moveSpeed * forceMultiplier * airMultiplier,
                ForceMode.Force
            );
        }
    }

    private void SpeedControl()
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (horizontalVelocity.magnitude > moveSpeed)
        {
            Vector3 cappedVelocity = horizontalVelocity.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(cappedVelocity.x, rb.linearVelocity.y, cappedVelocity.z);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        canJump = true;
    }
}