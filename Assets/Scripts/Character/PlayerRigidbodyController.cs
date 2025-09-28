using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerRigidbodyController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 1f;
    public float jumpForce = 1f;
    public float fallMultiplier = 1f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundMask;

    [Header("References")]
    public Transform cam;

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
        // get cam forward direction and right direction
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0;
        camRight.y = 0;

        // move relative to cam direction
        Vector3 move = (camForward.normalized * vInput + camRight.normalized * hInput);
        Vector3 target = rb.position + move * speed * Time.fixedDeltaTime;
        rb.MovePosition(target);

        // rotate player to face movement direction
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.fixedDeltaTime);
        }


        // jump/fall logic
        bool isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        if (wantJump && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        wantJump = false;

        if (rb.linearVelocity.y < 0) // if falling
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
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
