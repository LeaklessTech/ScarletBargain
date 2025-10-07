using System;
using UnityEngine;
using UnityEngine.AI;
using Utils;

public class MonsterBehavior : MonoBehaviour
{
    // Parent Tree
    public BehaviorTree Tree;
    public Node.Status TreeStatus = Node.Status.RUNNING;

    public bool IsStunned = false;
    public bool IsCharacterFound = false;

    private NavMeshAgent agent;
    private Animator anim;

    private Vector3 characterPosition;

    // Describes whether or not an action is currently active or not, separate from a Node Status
    public enum ActionState { IDLE, WORKING };
    ActionState state = ActionState.IDLE;

    void Start()
    {
        FieldOfView.OnPlayerFound += TriggerHunt;
        agent = this.GetComponent<NavMeshAgent>();
        anim = this.GetComponentInChildren<Animator>();

        // AI Behavior Setup
        Tree = new BehaviorTree("Base Tree");

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
        huntSequence.AddChild(new Leaf("Consume", Consume));
        huntSequence.AddChild(new Leaf("Victory", Victory));
        huntLook.AddChild(huntSequence);

        Sequence failHuntSequence = new Sequence("Failed to Hunt");
        failHuntSequence.AddChild(new Leaf("Look Around", Swivel));
        failHuntSequence.AddChild(new Leaf("Anger", Angry));
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

    private void TriggerHunt(Vector3 vector3)
    {
        characterPosition = vector3;
    }


    void Update()
    {
        if (TreeStatus != Node.Status.SUCCESS)
            TreeStatus = Tree.Process();
    }

    #region Behaviors
    public Node.Status Swivel()
    {
        if (state == ActionState.IDLE)
        {
            anim.Play("Swivel", -1, 0f);
            state = ActionState.WORKING;
        }

        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Swivel"))
        {
            state = ActionState.IDLE;
            return Node.Status.SUCCESS;
        }

        return Node.Status.RUNNING;
    }

    public Node.Status Patrol()
    {
        return GoToLocation(WaypointsManager.Instance.GetWaypoint());
    }

    public Node.Status DestroyItem()
    {
        throw new NotImplementedException();
    }

    public Node.Status HuntCharacter()
    {
        if (characterPosition == null || characterPosition == Vector3.zero)
        {
            state = ActionState.IDLE;
            return Node.Status.FAILURE;
        }




        return Node.Status.RUNNING;
    }

    public Node.Status Consume()
    {
        throw new NotImplementedException();
    }

    public Node.Status Angry()
    {
        throw new NotImplementedException();
    }

    public Node.Status Victory()
    {
        throw new NotImplementedException();
    }

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
        return IsCharacterFound ? Node.Status.SUCCESS : Node.Status.FAILURE;
    }
    #endregion


    #region Actions
    Node.Status GoToLocation(Vector3 destination)
    {
        if (state == ActionState.IDLE)
        {
            agent.SetDestination(destination);
            state = ActionState.WORKING;
        }
        // Not checking for failure to reach right now
        // else if (Vector3.Distance(agent.pathEndPosition, destination) >= 2)
        // {
        //     state = ActionState.IDLE;
        //     print("FAILURE TO REACH");
        //     return Node.Status.FAILURE;
        // }
        else if (NavMeshUtilities.IsAtTargetLocation(agent))
        {
            state = ActionState.IDLE;
            return Node.Status.SUCCESS;
        }

        return Node.Status.RUNNING;
    }
    #endregion
}
