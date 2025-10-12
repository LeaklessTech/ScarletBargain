using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class AdvancedPlayerController : MonoBehaviour
{
    [Header("movement speeds")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float crouchSpeed = 1.2f;
    public float jumpForce = 3f;
    public float gravityMultiplier = 2f;

    [Header("ground check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundMask;

    [Header("references")]
    public Transform cam;

    [Header("keys")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;
    // public KeyCode interactKey = KeyCode.E;

    [Header("crouch height")]
    [Range(0.1f, 1f)]
    public float crouchHeightFactor = 0.2f;

    // internal
    Rigidbody rb;
    CapsuleCollider col;
    float hInput;
    float vInput;
    bool wantJump;
    bool isCrouching;
    bool isRunning;
    bool isHiding;
    bool isActive = true;
    float originalHeight;
    Vector3 originalCenter;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        originalHeight = col.height;
        originalCenter = col.center;
    }

    void Update()
    {
        if (!isActive) return;

        // gather movement input on the horizontal axes
        hInput = Input.GetAxisRaw("Horizontal");
        vInput = Input.GetAxisRaw("Vertical");

        // toggle crouch on key press
        if (Input.GetKeyDown(crouchKey))
        {
            ToggleCrouch();
        }

        // sprint is held down
        isRunning = Input.GetKey(sprintKey) && !isCrouching;

        // register jump request
        if (Input.GetButtonDown("Jump"))
        {
            wantJump = true;
        }
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        // determine movement direction relative to camera orientation
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0f;
        camRight.y = 0f;

        Vector3 moveDir = (camForward.normalized * vInput + camRight.normalized * hInput);
        if (moveDir.magnitude > 1f) moveDir.Normalize();

        // determine the appropriate speed
        float targetSpeed = walkSpeed;
        if (isCrouching)
            targetSpeed = crouchSpeed;
        else if (isRunning)
            targetSpeed = runSpeed;

        // move the rigidbody using MovePosition for smooth collision resolution
        Vector3 displacement = moveDir * targetSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + displacement);

        // rotate to face movement direction
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.fixedDeltaTime);
        }

        // ground detection
        bool grounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

        // jumping – assign vertical velocity only when grounded
        if (wantJump && grounded && !isHiding)
        {
            // apply instantaneous vertical velocity for jumping
            Vector3 vel = rb.linearVelocity;
            vel.y = jumpForce;
            rb.linearVelocity = vel;
        }
        wantJump = false;

        // apply extra gravity when falling for a snappier feel
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (gravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }


    void ToggleCrouch()
    {
        if (isHiding) return;
        isCrouching = !isCrouching;
        if (isCrouching)
        {
            col.height = originalHeight * crouchHeightFactor;
            col.center = originalCenter * crouchHeightFactor;
        }
        else
        {
            col.height = originalHeight;
            col.center = originalCenter;
        }
    }


    public void SetActive(bool active)
    {
        isActive = active;
    }


    public void EnterHideSpot()
    {
        if (isHiding) return;
        isHiding = true;
        rb.isKinematic = true;
    }


    public void ExitHideSpot()
    {
        if (!isHiding) return;
        isHiding = false;
        rb.isKinematic = false;
    }


    public bool IsHiding()
    {
        return isHiding;
    }
}