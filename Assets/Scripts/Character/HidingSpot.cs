using UnityEngine;

// attach to collider to set as a hiding spot
public class HidingSpot : MonoBehaviour
{
    [Tooltip("hide key")]
    public KeyCode hideKey = KeyCode.Z;

    private AdvancedPlayerController currentPlayer;
    private bool playerInside;

    void OnTriggerEnter(Collider other)
    {
        var controller = other.GetComponent<AdvancedPlayerController>();
        if (controller != null)
        {
            currentPlayer = controller;
            playerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        var controller = other.GetComponent<AdvancedPlayerController>();
        if (controller != null && controller == currentPlayer)
        {
            // auto unhide when leaving the hiding spot
            if (controller.IsHiding())
            {
                controller.ExitHideSpot();
            }
            currentPlayer = null;
            playerInside = false;
        }
    }

    void Update()
    {
        if (!playerInside || currentPlayer == null) return;
        // only react to input when this character is the active one
        if (Input.GetKeyDown(hideKey))
        {
            if (currentPlayer.IsHiding())
            {
                currentPlayer.ExitHideSpot();
            }
            else
            {
                currentPlayer.EnterHideSpot();
            }
        }
    }
}