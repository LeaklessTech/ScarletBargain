using UnityEngine;


public class RescuablePrisoner : MonoBehaviour
{
    [Tooltip("key used for rescue")]
    public KeyCode rescueKey = KeyCode.E;

    [Tooltip("tag assigned to the player character(s) for rescue detection")]
    public string playerTag = "Player";

    [Tooltip("reference to the PrisonerAI component")]
    public PrisonerAI prisonerAI;

    // track whether a player is inside the trigger
    bool playerNearby;
    Transform nearestPlayer;

    void Reset()
    {
        // auto assign the PrisonerAI if present
        if (prisonerAI == null)
            prisonerAI = GetComponent<PrisonerAI>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerNearby = true;
            nearestPlayer = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerNearby = false;
            nearestPlayer = null;
        }
    }

    void Update()
    {
        if (!playerNearby || prisonerAI == null) return;

        // only allow rescue for unrescued prisoners
        if (!prisonerAI.isRescued && Input.GetKeyDown(rescueKey))
        {
            // add to the party manager
            PartyManager mgr = PartyManager.instance;
            if (mgr != null)
            {
                mgr.AddRescuedPrisoner(prisonerAI);
            }
            // start following the resceur
            prisonerAI.Rescue(nearestPlayer);
        }
    }
}