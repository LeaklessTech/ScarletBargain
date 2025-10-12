using UnityEngine;
using System.Collections.Generic;


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

        // enable only the first member; others act as followers until rescued
        for (int i = 0; i < partyMembers.Count; i++)
        {
            bool isActive = i == activeIndex;
            partyMembers[i].SetActive(isActive);
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

        // compute next index
        int nextIndex = (activeIndex + 1) % partyMembers.Count;

        // ssign NavMesh following to the previous leader
        PrisonerAI prevAI = current.GetComponent<PrisonerAI>();
        if (prevAI != null)
        {
            prevAI.Rescue(partyMembers[nextIndex].transform);
        }

        // update active index
        activeIndex = nextIndex;
        AdvancedPlayerController newLeader = partyMembers[activeIndex];
        newLeader.SetActive(true);

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
                ai.Rescue(newLeader.transform);
            }
        }
    }
}