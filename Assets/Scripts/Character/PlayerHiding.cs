using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerHiding : MonoBehaviour
{
    [Header("Hiding")]
    [SerializeField] private KeyCode hideKey = KeyCode.E;
    [SerializeField] private float exitHideDelay = 0f;
    [SerializeField] private float hideTransitionSpeed = 5f; // lerp speed to hide spot
    [SerializeField] private float exitTransitionSpeed = 5f; // lerp speed back from hide spot

    [Header("UI (Optional - Set to null to disable)")]
    [SerializeField] private TextMeshProUGUI hidePrompt; // world space UI text for "Press E to Hide"
    [SerializeField] private string hideText = "Press E to Hide";
    [SerializeField] private string exitText = "Press E to Exit";

    private AdvancedPlayerController playerController;
    private Animator animator;
    private HidingSpot currentHidingSpot;
    private bool wasCrouching = false;
    private float hideTimer = 0f;
    private Vector3 entryPosition; // store position when entering hide
    private bool isExitingHide = false; // flag for exit crawl phase
    private Vector3 exitTargetPosition; // target for exit lerp (starts as entryPosition)

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
        if (Input.GetKeyDown(hideKey) && !isExitingHide)
        {
            // HIDE_DEBUG: Log key press with spot details
            Debug.Log($"[HIDE_DEBUG] Hide key ({hideKey}) pressed. Current hidden state: {IsHidden}. Spot available: {currentHidingSpot != null} (spot: {(currentHidingSpot != null ? currentHidingSpot.GetSpotName() : "None")})");

            if (!IsHidden && currentHidingSpot != null)
            {
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

        if (IsHidden && currentHidingSpot != null && !isExitingHide)
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

        if (isExitingHide)
        {
            if (exitTransitionSpeed > 0)
            {
                transform.position = Vector3.Lerp(transform.position, exitTargetPosition, exitTransitionSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = exitTargetPosition;
            }

            if (Vector3.Distance(transform.position, exitTargetPosition) < 0.1f)
            {
                CompleteExitHideSpot();
            }
        }
    }

    public void EnterHideSpot(HidingSpot spot)
    {
        if (IsHidden || spot == null || playerController == null) return;

        currentHidingSpot = spot;
        hideTimer = 0f;
        entryPosition = transform.position; // store the position where hiding started

        wasCrouching = playerController.IsCrouching;

        playerController.EnterHideSpot();

        if (animator != null)
        {
            animator.SetBool("IsCrawling", true);
            animator.speed = 1f;
        }

        UpdatePrompt(exitText, true);

        // HIDE_DEBUG: Log successful hide start
        Debug.Log($"[HIDE_DEBUG] Hiding started! Stored entry pos: {entryPosition}. Lerping to {spot.GetSpotName()} at {spot.HidePosition.position}. Kinematic: {playerController.gameObject.GetComponent<Rigidbody>().isKinematic}, Crawl anim: true");
    }

    public void ExitHideSpot()
    {
        if (!IsHidden || playerController == null || isExitingHide) return;

        isExitingHide = true;
        exitTargetPosition = entryPosition;

        if (animator != null)
        {
            animator.speed = -1f;
        }

        if (animator != null)
        {
            animator.SetBool("IsCrawling", true);
        }

        // HIDE_DEBUG: Log exit start
        Debug.Log($"[HIDE_DEBUG] Exit crawl started! Lerping back to {entryPosition} (distance from hide: {Vector3.Distance(transform.position, entryPosition):F2}). Anim speed: {animator?.speed}");
    }

    private void CompleteExitHideSpot()
    {
        isExitingHide = false;

        playerController.ExitHideSpot();

        if (animator != null)
        {
            animator.SetBool("IsCrawling", false);
            animator.speed = 1f;
        }

        currentHidingSpot?.ExitHideSpot(this);
        currentHidingSpot = null;
        hideTimer = 0f;

        // hide prompt
        UpdatePrompt(null, false);

        // HIDE_DEBUG: Log exit complete
        Debug.Log($"[HIDE_DEBUG] Exit crawl complete! Position restored to {transform.position} (target was {entryPosition}). Kinematic: {playerController.gameObject.GetComponent<Rigidbody>().isKinematic}, Crawl anim: false");
    }

    public void SetCurrentSpot(HidingSpot spot)
    {
        currentHidingSpot = spot;
        // HIDE_DEBUG: Log spot reference set
        Debug.Log($"[HIDE_DEBUG] Current hiding spot set to: {(spot != null ? spot.GetSpotName() : "None")}");
    }

    public void ShowHidePrompt(HidingSpot spot)
    {
        if (IsHidden || isExitingHide || hidePrompt == null) return;

        SetCurrentSpot(spot);
        UpdatePrompt(hideText, true);
    }

    public void HidePrompt()
    {
        if (!IsHidden && !isExitingHide && hidePrompt != null)
        {
            hidePrompt.gameObject.SetActive(false);
        }
    }

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
