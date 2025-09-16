using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerRigidbodyController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;
    public float jumpForce = 5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundMask;

    Rigidbody rb;
    float hInput, vInput;
    bool wantJump = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        hInput = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        vInput = Input.GetAxisRaw("Vertical");   // W/S or Up/Down

        if (Input.GetButtonDown("Jump"))
            wantJump = true;
    }

    void FixedUpdate()
    {
        // move
        Vector3 move = (transform.right * hInput + transform.forward * vInput).normalized;
        Vector3 target = rb.position + move * speed * Time.fixedDeltaTime;
        rb.MovePosition(target);

        bool isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        if (wantJump && isGrounded)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = jumpForce;
            rb.linearVelocity = vel;
        }

        wantJump = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
