using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Orbit & Input")]
    public float mouseSensitivity = 2f;
    public bool lockCursorOnStart = true;

    [Header("Distance")]
    public float distance = 2f;
    public float minDistance = 0.6f;
    public float maxDistance = 2.5f;

    [Header("Pitch Limits")]
    public float minPitch = -15f;
    public float maxPitch = 30f;

    [Header("Collision (occlusion)")]
    public LayerMask obstructionMask = ~0; // default: everything
    public float cameraRadius = 0.2f;      // radius for spherecast
    public float collisionPadding = 0.05f; // small offset so camera doesn't touch geometry

    // runtime
    float yaw;
    float pitch;
    float yawVelocity;
    float pitchVelocity;
    Vector3 posVelocity;

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;

        if (target == null)
        {
            Debug.LogWarning("ThirdPersonCamera: target not assigned. Please assign target (CameraTarget).");
            enabled = false;
            return;
        }

        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x;
        SetCursorLocked(lockCursorOnStart);
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");
        yaw += mx * mouseSensitivity;
        pitch += -1f * my * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 targetPos = target.position + targetOffset;

        // desired cam position
        Vector3 desiredLocal = rotation * new Vector3(0f, 0f, -Mathf.Clamp(distance, minDistance, maxDistance));
        Vector3 desiredWorldPos = targetPos + desiredLocal;

        // collision: spherecast from target to desired position
        Vector3 dir = (desiredWorldPos - targetPos).normalized;
        float desiredDist = Vector3.Distance(targetPos, desiredWorldPos);

        RaycastHit hit;
        float finalDistance = desiredDist;
        if (Physics.SphereCast(targetPos, cameraRadius, dir, out hit, desiredDist, obstructionMask, QueryTriggerInteraction.Ignore))
        {
            // push camera closer than the hit point (with padding)
            finalDistance = Mathf.Clamp(hit.distance - cameraRadius - collisionPadding, minDistance, desiredDist);
        }

        Vector3 correctedWorldPos = targetPos + rotation * new Vector3(0f, 0f, -finalDistance);

        // smooth position (change float value)
        transform.position = Vector3.SmoothDamp(transform.position, correctedWorldPos, ref posVelocity, 0.08f);

        // smooth look-at (so rotation doesn't snap when camera hits geometry)
        Vector3 lookTarget = targetPos;
        Quaternion desiredRot = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, 1f - Mathf.Exp(-20f * Time.deltaTime));
    }

    public void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(target.position + targetOffset, cameraRadius);

        // draw desired and corrected positions
        Quaternion debugRot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desired = target.position + targetOffset + debugRot * new Vector3(0, 0, -distance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(desired, 0.05f);
        Gizmos.color = Color.yellow;
        // draw line from target to desired
        Gizmos.DrawLine(target.position + targetOffset, desired);
    }
}
