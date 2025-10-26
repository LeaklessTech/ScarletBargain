using System;
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

    public WaypointListReference waypointList;
    private UnityEngine.Vector3 characterPosition;
    private UnityEngine.Vector3 prevCharacterPosition;

    private int huntedPlayerId;

    private Waypoint previousWaypoint;

    // Describes whether or not an action is currently active or not, separate from a Node Status
    public enum ActionState { IDLE, WORKING };
    ActionState state = ActionState.IDLE;

    System.Random rnd = new System.Random();

    void Start()
    {
        // FieldOfView.OnPlayerFound += TriggerHunt;
        agent = this.GetComponent<NavMeshAgent>();
        anim = this.GetComponentInChildren<Animator>();

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
        patrolSequence.AddChild(new Leaf("Look Around", Swivel));


        Tree.AddChild(stunSequence);
        Tree.AddChild(chaseSequence);
        Tree.AddChild(patrolSequence);

        Tree.PrintTree();
    }

    void Update()
    {
        TreeStatus = Tree.Process();
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
            anim.Play("MonsterIdle");
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

        // If the monster has not lost sight of the character then keep trying to find them
        if (prevCharacterPosition != characterPosition)
        {
            state = ActionState.WORKING;
            agent.SetDestination(characterPosition);
        }

        if (Vector3.Distance(this.transform.position, characterPosition) < 2)
        {
            Debug.Log("Conusmed character in hunt stage");
            onCharacterKilled.TriggerEvent(this, huntedPlayerId);
            onPlayMonsterAudio.TriggerEvent(this, Resources.Load<AudioClip>("Audio/monster-victory"));
            state = ActionState.IDLE;
            IsCharacterFound = false;
            return Node.Status.SUCCESS; // Return failure here because 
        }
        if (NavMeshUtilities.IsAtTargetLocation(agent))
        {
            // TODO: need to play kill animation then disable/destroy the character that was killed
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
