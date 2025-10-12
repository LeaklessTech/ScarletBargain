using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class PrisonerAI : MonoBehaviour
{
    [Tooltip("min distance to keep from the leader when following.")]
    public float followDistance = 1.5f;

    // flag indicating whether the prisoner has been rescued
    public bool isRescued { get; private set; }

    Transform leader;
    NavMeshAgent agent;
    AdvancedPlayerController controller;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        controller = GetComponent<AdvancedPlayerController>();
        if (controller != null)
        {
            // do not allow unrescued prisoners to be controlled
            controller.enabled = false;
            controller.SetActive(false);
        }
        // initially do not move
        agent.isStopped = true;
    }

    void Update()
    {
        if (!isRescued || leader == null) return;

        float dist = Vector3.Distance(transform.position, leader.position);
        if (dist > followDistance)
        {
            agent.isStopped = false;
            // continuously update the destination so the agent will recompute the path if the leader moves
            agent.SetDestination(leader.position);
        }
        else
        {
            // stop moving if close enough
            agent.isStopped = true;
        }
    }

    public void Rescue(Transform newLeader)
    {
        isRescued = true;
        leader = newLeader;
        if (controller != null)
        {
            controller.enabled = true;
        }
    }
}