using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class AdvancedPlayerController : MonoBehaviour
{
    [Header("movement speeds")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float crouchSpeed = 1.2f;
    // public float jumpForce = 4.5f;
    public float gravityMultiplier = 3f;

    [Header("ground check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundMask;

    [Header("references")]
    public GameObject cam;

    [Header("keys")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;
    // public KeyCode interactKey = KeyCode.E;

    // [SerializeField, Range(0f, 1f)]
    // private float crouchHeightFactor = 0.2f;

    [SerializeField, Range(0f, 89f)] private float maxGroundAngle = 60f;
    //[SerializeField] private string groundTag = "Ground";

    public bool IsMoving => Mathf.Abs(hInput) > 0.05f || Mathf.Abs(vInput) > 0.05f;
    public bool IsCrouching => isCrouching;
    public bool IsRunning => isRunning;

    // internal
    private Rigidbody rb;
    private CapsuleCollider col;

    private float hInput;
    private float vInput;

    private bool wantJump;
    private bool isCrouching;
    private bool isRunning;
    private bool isHiding;
    private bool isActive = true;
    private bool isGrounded;
    private float originalHeight;
    private Vector3 originalCenter;
    private Animator animator;

    private readonly HashSet<Collider> _groundContacts = new();
    private float _minGroundDot; // cos(maxGroundAngle)

    // for animation Speed param calculation
    private Vector3 _lastRbPos;

    void Start()
    {
        if (cam)
        {
            cam = Instantiate(cam);
        }

        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        originalHeight = col.height;
        originalCenter = col.center;
        _minGroundDot = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        animator = GetComponent<Animator>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        // if (animator) animator.applyRootMotion = false;

        // rigidbody stability settings
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        _lastRbPos = rb.position;
        if (!cam)
            Debug.LogWarning("AdvancedPlayerController: 'cam' reference is not set.", this);
    }

    private void OnValidate()
    {
        _minGroundDot = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        // crouchHeightFactor = Mathf.Clamp01(crouchHeightFactor);
        if (col)
        {
            if (col.height < 0.2f) col.height = 0.2f;
            if (col.radius < 0.05f) col.radius = 0.05f;
        }
    }

    void Update()
    {
        if (!isActive) return;

        // gather movement input
        hInput = Input.GetAxisRaw("Horizontal");
        vInput = Input.GetAxisRaw("Vertical");

        // toggle crouch
        if (Input.GetKeyDown(crouchKey))
        {
            ToggleCrouch();
        }
            
        // sprint is held down
        isRunning = Input.GetKey(sprintKey) && !isCrouching;

        // jump request
        /*
        if (Input.GetButtonDown("Jump"))
        {
            wantJump = true;
        }
        */

        // kills any tilt creep
        rb.angularVelocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        if (cam == null) return;

        // move relative to camera
        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;

        Vector3 moveDir = (camForward.normalized * vInput + camRight.normalized * hInput);
        if (moveDir.magnitude > 1f) moveDir.Normalize();

        // determine the appropriate speed
        float targetSpeed = isCrouching ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);

        // move the rigidbody using MovePosition for smooth collision resolution
        Vector3 displacement = moveDir * targetSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + displacement);

        // rotate to face movement direction
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            float maxTurn = 720f * Time.fixedDeltaTime;
            Quaternion next = Quaternion.RotateTowards(rb.rotation, targetRotation, maxTurn);
            rb.MoveRotation(next);
        }

        // gets speed with planar and tells animator controller the speed parameter, apparently using planar is more robust than linearvelocity
        if (animator)
        {
            Vector3 delta = rb.position - _lastRbPos;
            float planarSpeed = new Vector3(delta.x, 0f, delta.z).magnitude / Time.fixedDeltaTime;
            animator.SetFloat("Speed", planarSpeed, 0.3f, Time.deltaTime);
        }
        _lastRbPos = rb.position;

        if (!isGrounded)
        {
            isGrounded = ProbeGrounded();
        }

        // jumping
        /*
        if (wantJump && isGrounded && !isHiding)
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
        */
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    ProcessGroundCollision(collision);
    //}

    //private void OnCollisionStay(Collision collision)
    //{
    //    ProcessGroundCollision(collision);
    //}

    //private void OnCollisionExit(Collision collision)
    //{
    //    if (!collision.collider || !collision.collider.CompareTag(groundTag)) return;
    //    _groundContacts.Remove(collision.collider);
    //    isGrounded = _groundContacts.Count > 0;
    //}

    //private void ProcessGroundCollision(Collision collision)
    //{
    //    if (!collision.collider || !collision.collider.CompareTag(groundTag)) return;

    //    bool hasValidGroundNormal = false;
    //    int count = collision.contactCount;
    //    for (int i = 0; i < count; i++)
    //    {
    //        var n = collision.GetContact(i).normal;
    //        // Accept only reasonably-upward surfaces (reject walls/ceilings)
    //        if (n.y >= _minGroundDot)
    //        {
    //            hasValidGroundNormal = true;
    //            break;
    //        }
    //    }

    //    if (hasValidGroundNormal)
    //        _groundContacts.Add(collision.collider);
    //    else
    //        _groundContacts.Remove(collision.collider);

    //    isGrounded = _groundContacts.Count > 0;
    //}

    private bool ProbeGrounded()
    {
        float radius = Mathf.Max(0.01f, col.radius - 0.01f);
        Vector3 center = transform.TransformPoint(col.center);
        float half = (col.height * 0.5f) - radius;
        Vector3 bottom = center + Vector3.down * half;

        // short capsule just beneath the feet
        return Physics.CheckCapsule(bottom + Vector3.up * 0.02f, bottom + Vector3.up * 0.04f,
                                    radius, groundMask, QueryTriggerInteraction.Ignore);
    }


    void ToggleCrouch()
    {
        if (isHiding) return;

        isCrouching = !isCrouching;
        if (animator)
        {
            animator.SetBool("Crouched", isCrouching);
        }


        /*
        if (isCrouching)
        {
            col.height = originalHeight * crouchHeightFactor;
            col.center = new Vector3(originalCenter.x, originalCenter.y * crouchHeightFactor, originalCenter.z);
        }
        else
        {
            col.height = originalHeight;
            col.center = originalCenter;
        }
        */
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