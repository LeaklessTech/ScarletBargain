using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Utils;

public class MonsterBehavior : MonoBehaviour
{
    // Event Listeners
    [Header("Events")]
    public GameEvent onCharacterKilled;
    public GameEvent onPlayMonsterAudio;

    // Parent Tree
    public BehaviorTree Tree;
    public Node.Status TreeStatus = Node.Status.RUNNING;

    public bool IsStunned = false;
    public bool IsCharacterFound = false;

    private NavMeshAgent agent;
    private Animator anim;
    private FieldOfView fov;

    public WaypointListReference waypointList;
    private UnityEngine.Vector3 characterPosition;
    private UnityEngine.Vector3 prevCharacterPosition;

    private int huntedPlayerId;

    private Waypoint previousWaypoint;

    // Describes whether or not an action is currently active or not, separate from a Node Status
    public enum ActionState { IDLE, WORKING };
    ActionState state = ActionState.IDLE;

    System.Random rnd = new System.Random();

    private Animator animator;

    public int ScanRadius = 0;
    [SerializeField] public GameObject BotPrefab;
    [SerializeField] public GameObject EnergyBallPrefab;

    public AudioSource footstepSource;

    private float stepTimer = 0.2f;
    private float currStep;

    public AudioClip footstepClip;

    private GameObject playerObj;

    private float energyBallDelay = 0f;
    public float energyBallDelayMax = 10f;

    private float resetSpeed;

    private bool characterDied = false;



    void Start()
    {
        currStep = stepTimer;

        fov = this.GetComponent<FieldOfView>();
        agent = this.GetComponent<NavMeshAgent>();
        animator = this.GetComponent<Animator>();
        anim = this.GetComponent<Animator>();
        
        agent.speed = 6f;

        resetSpeed = 2f;

        // AI Behavior Setup
        Tree = new BehaviorTree("Base Tree", Policies.RunForever);

        // Stun Sequence
        Sequence stunSequence = new Sequence("Stun Sequence");
        stunSequence.AddChild(new Leaf("Is Stunned?", IsMonsterStunned));
        stunSequence.AddChild(new Leaf("Stunned", Stunned));

        // Chase Sequence
        Sequence chaseSequence = new Sequence("Chase Sequence");
        chaseSequence.AddChild(new Leaf("Character Found?", FoundCharacter));

        Selector huntLook = new Selector("Succeed Hunt or Fail Hunt");
        Sequence huntSequence = new Sequence("Hunt Sequence");
        huntSequence.AddChild(new Leaf("Hunt", HuntCharacter));
        huntLook.AddChild(huntSequence);

        Sequence failHuntSequence = new Sequence("Failed to Hunt");
        failHuntSequence.AddChild(new Leaf("Look Around", Swivel));
        huntLook.AddChild(failHuntSequence);

        chaseSequence.AddChild(huntLook);

        // Patrol Sequence
        Sequence patrolSequence = new Sequence("Patrol Sequence");
        patrolSequence.AddChild(new Leaf("Patrol", Patrol));
        // patrolSequence.AddChild(new Leaf("Look Around", Swivel));
        patrolSequence.AddChild(new Leaf("Scan", Scan));


        Tree.AddChild(stunSequence);
        Tree.AddChild(chaseSequence);
        Tree.AddChild(patrolSequence);

        Tree.PrintTree();

        playerObj = GameObject.FindWithTag("Player");
    }

    void Update()
    {

        if (energyBallDelay > 0)
        {
            energyBallDelay -= Time.deltaTime;
        }

        if (animator)
        {
            Vector3 vel = agent.velocity;
            float horizontalSpeed = new Vector3(vel.x, 0f, vel.z).magnitude;
            animator.SetFloat("Speed", horizontalSpeed, 0.1f, Time.deltaTime);
        }

        

        // discrete footsteps when moving
        // if (footstepSource && gameObject.GetComponent<Rigidbody>().linearVelocity.magnitude > 1)
        if (footstepSource && gameObject.GetComponent<NavMeshAgent>().speed > 1)
        {
            currStep -= Time.deltaTime;
            if (currStep <= 0f)
            {
                AudioClip clip = footstepClip;
                if (clip)
                {
                    footstepSource.pitch = UnityEngine.Random.Range(0.2f, 0.4f);
                    footstepSource.PlayOneShot(clip);
                }
                currStep = stepTimer;
            }
        }
        else
        {
            stepTimer = 0.2f;
        }


        TreeStatus = Tree.Process();

        // Reset speed after a small delay (monster can get stuck at zero speed if the attack animation does not finish)
        if(resetSpeed > 0 && agent.speed == 0)
        {
            resetSpeed -= Time.deltaTime;
        }

        if(resetSpeed <= 0)
        {
            agent.speed = 6f;
            resetSpeed = 2f;
        }
    }

    #region Behaviors
    public Node.Status Swivel()
    {

        if (state == ActionState.IDLE)
        {
            anim.Play("Swivel", -1, 0f);
            state = ActionState.WORKING;
            // If we don't return RUNNING here then we will get to the Animator check and succed, thus returning the monster to the idle animation without ever having played the Swivel animation
            return Node.Status.RUNNING;
        }

        if (!AnimatorIsPlaying("Swivel"))
        {
            state = ActionState.IDLE;
            anim.Play("Idle");
            return Node.Status.SUCCESS;
        }

        return Node.Status.RUNNING;
    }

    public Node.Status Patrol()
    {
        if (state == ActionState.IDLE)
        {
            previousWaypoint = GetWaypoint(previousWaypoint);
            agent.SetDestination(previousWaypoint.Position);
            state = ActionState.WORKING;
        }
        else if (NavMeshUtilities.IsAtTargetLocation(agent))
        {
            state = ActionState.IDLE;
            return Node.Status.SUCCESS;
        }

        return Node.Status.RUNNING;
    }

    public Node.Status Scan()
    {
        if (state == ActionState.IDLE && !IsCharacterFound)
        {
            
            state = ActionState.WORKING;

            List<Waypoint> targetList = new List<Waypoint>();
            // send out bots
            foreach (var waypoint in waypointList.WaypointListRef)
            {
                if (Math.Pow(waypoint.Position.x - this.transform.position.x, 2) + Math.Pow(waypoint.Position.z - this.transform.position.z, 2) < Math.Pow(ScanRadius, 2))
                {
                    targetList.Add(waypoint);
                }
            }

            targetList = targetList.OrderBy(x => Vector3.Distance(x.Position, this.transform.position)).ToList();
            targetList.RemoveAt(0); // remove target that monster is standing on

            foreach (var target in targetList)
            {
                Debug.LogWarning("Spawning BOT set for: " + target.Position);
                GameObject instance = Instantiate(BotPrefab.gameObject, this.transform.position, Quaternion.identity) as GameObject;
                instance.GetComponent<NavMeshAgent>().SetDestination(target.Position);
            }
            targetList.Clear();
        }

        // if all bots finished or player found
        if (GameObject.FindGameObjectsWithTag("Bot").Count() == 0 || IsCharacterFound)
        {
            // reset done scanning
            state = ActionState.IDLE;
            return Node.Status.SUCCESS;
        }
        
        

        return Node.Status.RUNNING;
    }

    public Node.Status DestroyItem()
    {
        throw new NotImplementedException();
    }

    public Node.Status HuntCharacter()
    {
        if (characterPosition == null || characterPosition == UnityEngine.Vector3.zero)
        {
            state = ActionState.IDLE;
            return Node.Status.FAILURE;
        }

        var playerHiding = playerObj.GetComponent<PlayerHiding>();
        if (playerHiding != null && playerHiding.IsHidden)
        {
            // Ignore hidden: Drop chase, resume patrol (feels like "lost sight")
            Debug.Log("[MONSTER_DEBUG] Player hidden—monster loses interest and resumes patrol.");
            IsCharacterFound = false;
            state = ActionState.IDLE;
            return Node.Status.FAILURE; // Fails huntLook → Bubbles to patrol
        }

        // If the monster has not lost sight of the character then keep trying to find them
        if (prevCharacterPosition != characterPosition)
        {
            state = ActionState.WORKING;
            agent.SetDestination(characterPosition);
        }

        if (Vector3.Distance(this.transform.position, characterPosition) < 8 && fov.CanSeePlayer && energyBallDelay <= 0)
        {
            anim.Play("Attack");
            energyBallDelay = energyBallDelayMax;
        }

        if(characterDied)
        {
            state = ActionState.IDLE;
            IsCharacterFound = false;
            characterDied = false;
            return Node.Status.SUCCESS;
        }

        if (NavMeshUtilities.IsAtTargetLocation(agent))
        {
            state = ActionState.IDLE;
            IsCharacterFound = false;
            onPlayMonsterAudio.TriggerEvent(this, Resources.Load<AudioClip>("Audio/monster-angry"));
            return Node.Status.FAILURE;
        }

        return Node.Status.RUNNING;
    }

    // public Node.Status Consume()
    // {


    //     if (Vector3.Distance(this.transform.position, characterPosition) < 2)
    //     {
    //         Debug.Log("Conusmed character");
    //         onCharacterKilled.TriggerEvent(this, huntedPlayerId);
    //         onPlayMonsterAudio.TriggerEvent(this, Resources.Load<AudioClip>("Audio/monster-victory"));
    //         state = ActionState.IDLE;
    //     }
    //     else
    //     {
    //         return Node.Status.FAILURE;
    //     }


    //     return Node.Status.SUCCESS;
    // }

    // public Node.Status Angry()
    // {
    //     Debug.Log("Angry yell");
    //     return Node.Status.SUCCESS;
    // }

    Node.Status IsMonsterStunned()
    {
        return IsStunned ? Node.Status.SUCCESS : Node.Status.FAILURE;
    }

    Node.Status Stunned()
    {
        throw new NotImplementedException();
    }

    Node.Status FoundCharacter()
    {
        // This Node will interrupt whatever the monster is doing, so we need to reset their actionstate to idle
        if (IsCharacterFound)
        {
            state = ActionState.IDLE;
            return Node.Status.SUCCESS;
        }
        
        return Node.Status.FAILURE;
    }
    #endregion

    #region Animation Events

    public void Fire()
    {
        var directionToTarget = new Vector3(characterPosition.x - this.transform.position.x, .75f, characterPosition.z - this.transform.position.z);
        directionToTarget.Normalize();

        GameObject instance = Instantiate(EnergyBallPrefab.gameObject, this.transform.position + new Vector3(0, 1f, 0.5f), Quaternion.identity);
        EnergyBall l = instance.GetComponent<EnergyBall>();
        l.huntedPlayerId = huntedPlayerId;
        instance.GetComponent<Rigidbody>().AddForce(directionToTarget * 750);
        onPlayMonsterAudio.TriggerEvent(this, Resources.Load<AudioClip>("Audio/monster-victory"));
    }

    public void BeginAttack()
    {
        agent.speed = 0;
    }

    public void EndAttack()
    {
        agent.speed = 6f;
    }

    #endregion


    #region Actions
    // Node.Status GoToLocation(Vector3 destination)
    // {
    //     if (state == ActionState.IDLE)
    //     {
    //         agent.SetDestination(destination);
    //         state = ActionState.WORKING;
    //     }
    //     // Not checking for failure to reach right now
    //     // else if (Vector3.Distance(agent.pathEndPosition, destination) >= 2)
    //     // {
    //     //     state = ActionState.IDLE;
    //     //     print("FAILURE TO REACH");
    //     //     return Node.Status.FAILURE;
    //     // }
    //     else if (NavMeshUtilities.IsAtTargetLocation(agent))
    //     {
    //         state = ActionState.IDLE;
    //         return Node.Status.SUCCESS;
    //     }

    //     return Node.Status.RUNNING;
    // }
    #endregion

    #region Helpers
    bool AnimatorIsPlaying()
    {
        return anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
    }

    bool AnimatorIsPlaying(string stateName)
    {
        return AnimatorIsPlaying() && anim.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    public void TriggerHunt(Component sender, object data)
    {
        if (data is CharacterPosition)
        {
            characterPosition = ((CharacterPosition)data).position;
            huntedPlayerId = ((CharacterPosition)data).objectId;
            IsCharacterFound = true;
        }
    }

    public void CharacterDied(Component sender, object data)
    {
        characterDied = true;
    }

    public Waypoint GetWaypoint(Waypoint prevWaypoint)
    {
        Waypoint removed = null;
        if (prevWaypoint != null)
        {
            removed = prevWaypoint;
            waypointList.WaypointListRef.Remove(prevWaypoint);
        }

        int totalWeight = waypointList.WaypointListRef.Sum(x => x.Weight);

        int randomNumber = rnd.Next(0, totalWeight);

        Waypoint selectedWaypoint = null;
        foreach (Waypoint waypoint in waypointList.WaypointListRef)
        {
            if (randomNumber < waypoint.Weight)
            {
                selectedWaypoint = waypoint;
                break;
            }

            randomNumber = randomNumber - waypoint.Weight;
        }

        if (removed != null)
        {
            waypointList.WaypointListRef.Add(removed);
        }

        return selectedWaypoint;
    }
    #endregion
}
