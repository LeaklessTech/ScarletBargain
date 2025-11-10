using UnityEngine;
using TMPro;

public class PlayerHiding : MonoBehaviour
{
    [Header("Hiding")]
    [SerializeField] private KeyCode hideKey = KeyCode.E;
    [SerializeField] private float exitHideDelay = 0f; // 0 = no auto-exit, e.g., 10f for 10-sec timer
    [SerializeField] private float hideTransitionSpeed = 5f; // Lerp speed to hide spot (0 = instant)

    [Header("UI (Optional - Set to null to disable)")]
    [SerializeField] private TextMeshProUGUI hidePrompt; // World Space UI Text for "Press E to Hide"
    [SerializeField] private string hideText = "Press E to Hide";
    [SerializeField] private string exitText = "Press E to Exit";

    private AdvancedPlayerController playerController;
    private Animator animator;
    private HidingSpot currentHidingSpot;
    private bool wasCrouching = false; // Track prior state via public getter
    private float hideTimer = 0f;

    public bool IsHidden => playerController?.IsHiding() ?? false;

    private void Awake()
    {
        playerController = GetComponent<AdvancedPlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerHiding requires AdvancedPlayerController on the same GameObject!");
            return;
        }

        animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("PlayerHiding: No Animator found—hiding animations will be skipped.");
        }
    }

    private void Update()
    {
        // Handle input (centralized here; overrides HidingSpot's Update if conflicting)
        if (Input.GetKeyDown(hideKey))
        {
            // HIDE_DEBUG: Log key press with spot details
            Debug.Log($"[HIDE_DEBUG] Hide key ({hideKey}) pressed. Current hidden state: {IsHidden}. Spot available: {currentHidingSpot != null} (spot: {(currentHidingSpot != null ? currentHidingSpot.GetSpotName() : "None")})");

            if (!IsHidden && currentHidingSpot != null)
            {
                // Check spot occupancy
                if (currentHidingSpot.TryEnterHideSpot(this))
                {
                    EnterHideSpot(currentHidingSpot);
                }
                else
                {
                    // HIDE_DEBUG: Log failure (e.g., occupied)
                    Debug.LogWarning($"[HIDE_DEBUG] Failed to hide: Spot {currentHidingSpot.GetSpotName()} is occupied.");
                }
            }
            else if (IsHidden)
            {
                ExitHideSpot();
            }
            else
            {
                // HIDE_DEBUG: Log why hiding didn't start (no spot)
                Debug.LogWarning($"[HIDE_DEBUG] Hide key pressed but no hiding spot available. Enter a trigger first!");
            }
        }

        // While hidden: Lerp position and check auto-exit
        if (IsHidden && currentHidingSpot != null)
        {
            Transform targetPos = currentHidingSpot.HidePosition;
            if (targetPos != null)
            {
                if (hideTransitionSpeed > 0)
                {
                    transform.position = Vector3.Lerp(transform.position, targetPos.position, hideTransitionSpeed * Time.deltaTime);
                }
                else
                {
                    transform.position = targetPos.position;
                }
            }

            if (exitHideDelay > 0)
            {
                hideTimer += Time.deltaTime;
                if (hideTimer >= exitHideDelay)
                {
                    ExitHideSpot();
                }
            }
        }
    }

    // Called by HidingSpot on input
    public void EnterHideSpot(HidingSpot spot)
    {
        if (IsHidden || spot == null || playerController == null) return;

        currentHidingSpot = spot;
        hideTimer = 0f;

        // Track prior crouch state (public getter)
        wasCrouching = playerController.IsCrouching;

        // Use controller's hiding (sets kinematic)
        playerController.EnterHideSpot();

        // Force crouch animation directly (bypasses ToggleCrouch)
        if (animator != null)
        {
            animator.SetBool("Crouched", true);
        }

        // Optional prompt
        UpdatePrompt(exitText, true);

        // HIDE_DEBUG: Log successful hide start
        Debug.Log($"[HIDE_DEBUG] Hiding started! Lerping to {spot.GetSpotName()} at {spot.HidePosition.position}. Kinematic: {playerController.gameObject.GetComponent<Rigidbody>().isKinematic}, Crouch anim: true");
    }

    // Called by HidingSpot on input or trigger exit
    public void ExitHideSpot()
    {
        if (!IsHidden || playerController == null) return;

        // Use controller's unhiding (unsets kinematic)
        playerController.ExitHideSpot();

        // Restore crouch animation directly
        if (animator != null)
        {
            animator.SetBool("Crouched", wasCrouching);
        }

        // Notify spot (frees occupancy)
        currentHidingSpot?.ExitHideSpot(this);
        currentHidingSpot = null;
        hideTimer = 0f;

        // Hide prompt
        UpdatePrompt(null, false);

        // HIDE_DEBUG: Log exit
        Debug.Log($"[HIDE_DEBUG] Hiding ended! Restored crouch: {wasCrouching}. Kinematic: {playerController.gameObject.GetComponent<Rigidbody>().isKinematic}");
    }

    public void SetCurrentSpot(HidingSpot spot)
    {
        currentHidingSpot = spot;
        // HIDE_DEBUG: Log spot reference set
        Debug.Log($"[HIDE_DEBUG] Current hiding spot set to: {(spot != null ? spot.GetSpotName() : "None")}");
    }

    // Optional: Called by HidingSpot OnTriggerEnter (for prompt only)
    public void ShowHidePrompt(HidingSpot spot)
    {
        if (IsHidden || hidePrompt == null) return;

        SetCurrentSpot(spot);
        UpdatePrompt(hideText, true);
    }

    // Optional: Called by HidingSpot OnTriggerExit
    public void HidePrompt()
    {
        if (!IsHidden && hidePrompt != null)
        {
            hidePrompt.gameObject.SetActive(false);
        }
        // Don't null spot here—let SetCurrentSpot(null) handle in OnTriggerExit
    }

    // Internal: Update prompt text/position
    private void UpdatePrompt(string text, bool active)
    {
        if (hidePrompt == null) return;

        hidePrompt.gameObject.SetActive(active);
        if (active && text != null)
        {
            hidePrompt.text = text.Replace("E", hideKey.ToString());
            if (currentHidingSpot != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(currentHidingSpot.transform.position + Vector3.up * 1f);
                hidePrompt.rectTransform.position = screenPos;
            }
        }
    }
}