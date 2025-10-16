using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;


public class PartyManager : MonoBehaviour
{
    public static PartyManager instance;

    [Tooltip("ref to the third person camera script")]
    public ThirdPersonCam cameraScript;

    [Tooltip("initial party members")]
    public List<AdvancedPlayerController> partyMembers = new List<AdvancedPlayerController>();

    // index of the currently controlled character in the partyMembers list
    int activeIndex;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (partyMembers.Count == 0)
        {
            Debug.LogWarning("PartyManager: no party members assigned.");
            return;
        }

        // enable only the first member; others will be followers when they become active
        for (int i = 0; i < partyMembers.Count; i++)
        {
            bool isActive = i == activeIndex;
            partyMembers[i].SetActive(isActive);

            PrisonerAI ai = partyMembers[i].GetComponent<PrisonerAI>();
            if (ai != null)
            {
                ai.enabled = false;
            }
        }
        // set the camera to look at the active member
        if (cameraScript != null)
        {
            cameraScript.target = partyMembers[activeIndex].transform;
        }
    }

    void Update()
    {
        // cycle party with Tab key
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SwitchCharacter();
        }
    }

    // adds a rescued prisoner to the party
    public void AddRescuedPrisoner(PrisonerAI prisoner)
    {
        if (prisoner == null) return;
        AdvancedPlayerController ctrl = prisoner.GetComponent<AdvancedPlayerController>();
        if (ctrl == null)
        {
            Debug.LogWarning("PartyManager: rescued object has no AdvancedPlayerController.");
            return;
        }
        if (!partyMembers.Contains(ctrl))
        {
            partyMembers.Add(ctrl);
            // new member is inactive until selected
            ctrl.SetActive(false);
            // immediately start following the current leader
            prisoner.enabled = true;
            prisoner.Rescue(partyMembers[activeIndex].transform);
        }
    }

    // switches to next character in party
    void SwitchCharacter()
    {
        if (partyMembers.Count <= 1) return;

        // disable current controller input
        AdvancedPlayerController current = partyMembers[activeIndex];
        current.SetActive(false);

        // compute next index (the new leader)
        int nextIndex = (activeIndex + 1) % partyMembers.Count;

        // enable the AI on the previous leader so it will follow the new leader
        PrisonerAI prevAI = current.GetComponent<PrisonerAI>();
        if (prevAI != null)
        {
            // enable AI script when a character becomes a follower
            prevAI.enabled = true;
            prevAI.Rescue(partyMembers[nextIndex].transform);
        }

        // update active index
        activeIndex = nextIndex;
        AdvancedPlayerController newLeader = partyMembers[activeIndex];
        newLeader.SetActive(true);

        // disable the AI on the newly controlled character so it doesn't interferes
        PrisonerAI newAI = newLeader.GetComponent<PrisonerAI>();
        if (newAI != null)
        {
            // disable the AI script for the leader and stop its NavMeshAgent
            newAI.enabled = false;
            NavMeshAgent nav = newAI.GetComponent<NavMeshAgent>();
            if (nav != null)
            {
                nav.isStopped = true;
            }
        }

        // update camera target
        if (cameraScript != null)
        {
            cameraScript.target = newLeader.transform;
        }

        // update all other followers to follow the new leader
        for (int i = 0; i < partyMembers.Count; i++)
        {
            if (i == activeIndex) continue;
            PrisonerAI ai = partyMembers[i].GetComponent<PrisonerAI>();
            if (ai != null)
            {
                // enable AI script when a character becomes a follower
                ai.enabled = true;
                ai.Rescue(newLeader.transform);
            }
        }
    }
}
