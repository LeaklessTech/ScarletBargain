using PSXShaderKit;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class AdvancedPlayerController : MonoBehaviour
{
    [Header("movement speeds")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float crouchSpeed = 1.2f;
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

    public string jumpAnimationTrigger = "Jump";
    public bool requireGroundedForJump = false;

    public AudioClip jumpSound;
    public AudioSource jumpAudioSource;

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

    // stamina
    public float maxStamina = 6f;
    public float staminaDrainRate = 1f;
    public float staminaRegenRate = 1f;
    public float minRunStamina = 2f;
    public Slider staminaSlider;

    public float jumpStaminaCost = 2f;
    public float minJumpStamina = 2f;

    // tracks current stamina value
    private float currentStamina;

    // true if player is exhausted (cannot run until stamina is >2 again)
    private bool staminaDepleted;

    // gets current stamina normalized 0-1 for ui
    public float GetStaminaNormalized()
    {
        if (maxStamina <= 0f) return 0f;
        return Mathf.Clamp01(currentStamina / maxStamina);
    }

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

        // initialize stamina to full
        currentStamina = maxStamina;

        // the player starts with full stamina, so they are not exhausted
        staminaDepleted = false;

        // if a UI slider is assigned, configure its range and initial value
        if (staminaSlider != null)
        {
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = 1f;
            staminaSlider.value = 1f;
        }

        if (jumpSound != null && jumpAudioSource == null)
        {
            jumpAudioSource = gameObject.AddComponent<AudioSource>();
        }
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

        // determine if the player wants to run (sprint key held and not crouching)
        bool wantsToRun = Input.GetKey(sprintKey) && !isCrouching;
        // is the character currently moving? we only allow sprinting when actually moving
        bool isCurrentlyMoving = IsMoving;

        HandleStaminaAndRunning(wantsToRun, isCurrentlyMoving);

        // jump
        if (Input.GetButtonDown("Jump"))
        {
            TryPlayJumpAnimation();
        }

        // kills any tilt creep
        rb.angularVelocity = Vector3.zero;

        // update the stamina UI slider if assigned
        if (staminaSlider != null)
        {
            staminaSlider.value = GetStaminaNormalized();
        }
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

        // apply extra gravity when falling for a snappier feel
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (gravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

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

    private void HandleStaminaAndRunning(bool wantsToRun, bool isCurrentlyMoving)
    {
        // if the stamina system is disabled (maxStamina <= 0) just reflect the input state
        if (maxStamina <= 0f)
        {
            isRunning = wantsToRun && isCurrentlyMoving;
            return;
        }

        if (staminaDepleted)
        {
            // cannot sprint while exhausted
            isRunning = false;
            // regenerate stamina at the defined rate
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina)
                currentStamina = maxStamina;
            // once regained enough stamina we can exit the exhausted state
            if (currentStamina >= minRunStamina)
            {
                staminaDepleted = false;
            }
            return;
        }

        // not exhausted: determine if the player is attempting to sprint
        if (wantsToRun && isCurrentlyMoving && currentStamina > 0f)
        {
            // the player is running; set the flag and drain stamina
            isRunning = true;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            // check for exhaustion
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                staminaDepleted = true;
                isRunning = false;
            }
        }
        else
        {
            // the player is not sprinting
            isRunning = false;
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina)
                currentStamina = maxStamina;
        }
    }


    void ToggleCrouch()
    {
        if (isHiding) return;

        isCrouching = !isCrouching;
        if (animator)
        {
            animator.SetBool("Crouched", isCrouching);
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

    private void TryPlayJumpAnimation()
    {
        // cannot jump while crouched
        if (isCrouching) return;

        // if there is no animator, nothing to play
        if (animator == null) return;

        // jump animation only plays when grounded (if selected)
        if (requireGroundedForJump && !isGrounded) return;

        // avoid jumping while hiding
        if (isHiding) return;

        // stamina check
        if (maxStamina > 0f)
        {
            if (currentStamina < minJumpStamina)
                return;                             // not enough stamina
            currentStamina -= jumpStaminaCost;      // drain stamina
            if (currentStamina < 0f) currentStamina = 0f;
            if (currentStamina <= 0f) staminaDepleted = true;
            if (staminaSlider != null)
                staminaSlider.value = GetStaminaNormalized();
        }

        // trigger jump in animator param set in 'jumpAnimationTrigger'
        if (!string.IsNullOrEmpty(jumpAnimationTrigger))
        {
            animator.SetTrigger(jumpAnimationTrigger);
        }

        jumpAudioSource.PlayOneShot(jumpSound);
    }
}