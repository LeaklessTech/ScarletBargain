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

    public Canvas notifyCanvas;
    private float wanderCooldown;

    // Flee settings
    [Header("Flee from Monster")]
    [Tooltip("If a monster is within this radius, the prisoner will try to flee away.")]
    public float monsterFearRadius = 8f;
    [Tooltip("How far to try to flee when a monster is nearby.")]
    public float fleeDistance = 12f;
    [Tooltip("Random jitter applied to flee target to avoid predictable paths.")]
    public float fleeJitter = 2f;


    void Awake()
    {
        notifyCanvas = GameObject.Find("PrisonerNotificationCanvas").GetComponent<Canvas>();

        notifyCanvas.enabled = false;

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
        //agent.isStopped = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isRescued)
            return;
        if (other.tag != "Player")
            return;

        notifyCanvas.enabled = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag != "Player")
            return;

        notifyCanvas.enabled = false;
    }

    void Update()
    {
        if (isDead)
            return;

        if (!isRescued || leader == null)
        {
            if (animator)
            {
                Vector3 vel = agent.velocity;
                float horizontalSpeed = new Vector3(vel.x, 0f, vel.z).magnitude;
                animator.SetFloat("Speed", horizontalSpeed, 0.1f, Time.deltaTime);
            }

            // if the agent is currently en route to a destination and still far from it, don't interrupt
            if (agent.remainingDistance > 2f && agent.destination != null)
                return;

            if (wanderCooldown > 0.1f)
            {
                wanderCooldown -= Time.deltaTime;
                return;
            }

            // If a monster is nearby, attempt to flee away from it instead of normal wandering.
            if (TryFleeFromNearestMonster())
                return;

            RandomWander();

            return;
        }

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

    // make the Prisoner wander around while not controlled
    public void RandomWander()
    {
        wanderCooldown = 5f;

        float walkRadius = 10f;

        Vector3 randomPosition = Random.insideUnitSphere * walkRadius;

        randomPosition += transform.position;
        NavMeshHit location;

        NavMesh.SamplePosition(randomPosition, out location, walkRadius, 1);

        Vector3 randomWalk = location.position;

        agent.SetDestination(randomWalk);

        if (animator)
        {
            Vector3 vel = agent.velocity;
            float horizontalSpeed = new Vector3(vel.x, 0f, vel.z).magnitude;
            animator.SetFloat("Speed", horizontalSpeed, 0.1f, Time.deltaTime);
        }
    }

    // Attempt to find the nearest monster and flee away from it.
    // Returns true if a flee destination was set.
    private bool TryFleeFromNearestMonster()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        if (monsters == null || monsters.Length == 0)
            return false;

        float fearSqr = monsterFearRadius * monsterFearRadius;
        GameObject nearest = null;
        float bestSqr = float.MaxValue;
        Vector3 myPos = transform.position;

        foreach (var m in monsters)
        {
            if (m == null) continue;
            float sq = (m.transform.position - myPos).sqrMagnitude;
            if (sq < bestSqr)
            {
                bestSqr = sq;
                nearest = m;
            }
        }

        if (nearest == null || bestSqr > fearSqr)
            return false;

        // compute flee direction (horizontal only)
        Vector3 fleeDir = (myPos - nearest.transform.position);
        fleeDir.y = 0f;
        if (fleeDir.sqrMagnitude < 0.01f)
        {
            // if overlapping, pick a random direction
            fleeDir = Random.insideUnitSphere;
            fleeDir.y = 0f;
        }
        fleeDir.Normalize();

        // apply jitter to avoid predictable straight-lines
        Vector3 jitter = Random.insideUnitSphere * fleeJitter;
        jitter.y = 0f;

        Vector3 target = myPos + fleeDir * fleeDistance + jitter;

        NavMeshHit hit;
        // allow a small extra search radius in case the exact spot isn't valid
        float sampleRange = fleeDistance + 4f;
        if (NavMesh.SamplePosition(target, out hit, sampleRange, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
            // short cooldown to avoid spamming decisions every frame
            wanderCooldown = 3f;
            return true;
        }

        return false;
    }
}