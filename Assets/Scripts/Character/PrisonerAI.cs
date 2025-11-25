using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class PrisonerAI : MonoBehaviour
{
    [Tooltip("min distance to keep from the leader when following.")]
    public float followDistance = 2f;

    // flag indicating whether the prisoner has been rescued
    public bool isRescued { get; private set; }

    public bool isDead = false;

    private float maxHistorySeconds = 20f; // max trail time history
    private float lookbackSeconds = 0.6f; // how far behind in seconds to follow leader
    private float sampleInterval = 0.05f; // how often leader pos is sampled
    private float minStepDistance = 0.05f; // record only if leader moved this far
    Transform leader;
    NavMeshAgent agent;
    Animator animator;
    AdvancedPlayerController controller;

    struct Sample { public Vector3 pos; public float time; }
    readonly List<Sample> history = new List<Sample>(256);
    float lastSampleTime;
    Vector3 lastSamplePos;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
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
        if (!isRescued || leader == null || isDead) return;

        float now = Time.time;

        // leader trail
        if (now - lastSampleTime >= sampleInterval)
        {
            Vector3 lp = leader.position;
            if (history.Count == 0 || (lp - lastSamplePos).sqrMagnitude >= minStepDistance * minStepDistance)
            {
                history.Add(new Sample { pos = lp, time = now });
                lastSamplePos = lp;
                lastSampleTime = now;
            }

            // trim old samples
            float cutoff = now - Mathf.Max(maxHistorySeconds, lookbackSeconds + 1f);
            while (history.Count > 1 && history[0].time < cutoff)
                history.RemoveAt(0);
        }

        // delayed target
        Vector3 delayedPos = leader.position; // fallback
        if (history.Count >= 2)
        {
            float targetTime = now - lookbackSeconds;

            if (targetTime <= history[0].time)
            {
                delayedPos = history[0].pos;
            }
            else
            {
                // find segment [i, i+1] that straddles targetTime
                for (int i = history.Count - 2; i >= 0; i--)
                {
                    if (history[i].time <= targetTime)
                    {
                        var a = history[i];
                        var b = history[i + 1];
                        float t = Mathf.InverseLerp(a.time, b.time, targetTime);
                        delayedPos = Vector3.Lerp(a.pos, b.pos, t);
                        break;
                    }
                }
            }
        }

        // follow to delayed target
        float dist = Vector3.Distance(transform.position, leader.position);
        if (dist > followDistance)
        {
            agent.isStopped = false;
            // continuously update the destination so the agent will recompute the path if the leader moves
            agent.SetDestination(delayedPos);
        }
        else
        {
            // stop moving if close enough
            agent.isStopped = true;
        }

        // feed speed float into animator param
        if (animator)
        {
            Vector3 vel = agent.velocity;
            float horizontalSpeed = new Vector3(vel.x, 0f, vel.z).magnitude;
            animator.SetFloat("Speed", horizontalSpeed, 0.1f, Time.deltaTime);
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

        history.Clear();
        float now = Time.time;
        Vector3 lp = leader.position;
        history.Add(new Sample { pos = lp, time = now - sampleInterval });
        history.Add(new Sample { pos = lp, time = now });
        lastSampleTime = now;
        lastSamplePos = lp;
    }
}