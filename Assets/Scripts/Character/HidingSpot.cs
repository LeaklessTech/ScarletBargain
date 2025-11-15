using UnityEngine;
using System;

// Attach to prop root (with trigger collider as child)
public class HidingSpot : MonoBehaviour
{
    [Tooltip("Hide key")]
    public KeyCode hideKey = KeyCode.E;

    [Header("Hide Setup")]
    [SerializeField] private Transform hidePosition; // Assign "HidePosition" child in prefab (position prop)
    [SerializeField] private float hideTransitionSpeed = 5f;
    [SerializeField] private bool isOccupied = false;

    [Header("Debug/Debounce")]
    [SerializeField] private float debounceTime = 0.2f; // Ignore exits for X seconds after enter (anti-jitter)

    private PlayerHiding currentHider;
    private bool playerInside;
    private DateTime lastEnterTime = DateTime.MinValue;

    private void Awake()
    {
        // Auto-setup hidePosition if missing
        if (hidePosition == null)
        {
            GameObject posObj = new GameObject("HidePosition");
            posObj.transform.SetParent(transform, false);
            posObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            hidePosition = posObj.transform;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var hider = other.GetComponent<PlayerHiding>();
        if (hider != null && !isOccupied)
        {
            currentHider = hider;
            playerInside = true;
            lastEnterTime = DateTime.Now;

            hider.SetCurrentSpot(this);

            // hider.ShowHidePrompt(this);

            // HIDE_DEBUG: Log entry into trigger
            Debug.Log($"[HIDE_DEBUG] Player entered hiding trigger for {gameObject.name} at position {transform.position}. Ready to hide! (Debounce reset)");
        }
        else if (hider == null)
        {
            Debug.Log($"[HIDE_DEBUG] Non-player {other.name} entered trigger for {gameObject.name}. Ignoring.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        var hider = other.GetComponent<PlayerHiding>();
        if (hider != null && hider == currentHider)
        {
            float timeSinceEnter = (float)(DateTime.Now - lastEnterTime).TotalSeconds;
            if (timeSinceEnter < debounceTime)
            {
                // HIDE_DEBUG: Log skipped exit due to debounce
                Debug.Log($"[HIDE_DEBUG] Trigger exit ignored for {gameObject.name} (jitter debounce: {timeSinceEnter:F2}s < {debounceTime}s).");
                return;
            }

            // if (hider.IsHidden) { hider.ExitHideSpot(); }  // Already commented

            if (!hider.IsHidden)
            {
                hider.SetCurrentSpot(null);
            }

            currentHider = null;
            playerInside = false;
            // hider.HidePrompt();

            // HIDE_DEBUG: Log exit with hidden state
            Debug.Log($"[HIDE_DEBUG] Player exited hiding trigger for {gameObject.name}. Hidden state: {hider.IsHidden}. (Auto-unhiding skipped if hidden.) Time since enter: {timeSinceEnter:F2}s");
        }
    }

    void Update()
    {
        if (!playerInside || currentHider == null) return;
    }

    public bool TryEnterHideSpot(PlayerHiding hider)
    {
        if (isOccupied) return false;
        isOccupied = true;
        return true;
    }

    public void ExitHideSpot(PlayerHiding hider)
    {
        isOccupied = false;
    }

    public Transform HidePosition => hidePosition;

    public KeyCode GetHideKey() => hideKey;
    public string GetSpotName() => name;
}