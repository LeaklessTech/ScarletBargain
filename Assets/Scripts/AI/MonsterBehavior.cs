using System;
using UnityEngine;
using UnityEngine.AI;
using Utils;

public class MonsterBehavior : MonoBehaviour
{
    // Parent Tree
    public BehaviorTree Tree;
    public Node.Status TreeStatus = Node.Status.RUNNING;

    // Specific Behavior Trees
    BehaviorTree chaseTree;
    BehaviorTree stunnedTree;
    BehaviorTree huntTree;
    public Node.Status ChaseStatus;
    public Node.Status StunStatus;

    private NavMeshAgent agent;
    private Animator anim;

    private Vector3 characterPosition;

    // Describes whether or not an action is currently active or not, separate from a Node Status
    public enum ActionState { IDLE, WORKING };
    ActionState state = ActionState.IDLE;

    /*
        PLAN:
        - patrol behavior: create a utility to handle patrol point gathering.
          - get list of points from parent object in the world
          - this list structure should have two objects per list item: object, value
            - this represents the position of the waypoint and then its value (this value will be used as a weight in the random choice of which waypoint to move to)
          - the utility will have a public method that returns a point (randomly generated with a value)
          - in the patrol behavior code, if the monster sees an object in vision cone for too long that has the tag Destructable then fail the patrol behavior (will automatically switch to the destroy behavior)
        - swivel behavior: play an animation that rotates the object
        - 

    */


    void Start()
    {
        FieldOfView.OnPlayerFound += TriggerHunt;
        agent = this.GetComponent<NavMeshAgent>();
        anim = this.GetComponentInChildren<Animator>();

        // Instantiate Chase Tree
        chaseTree = new BehaviorTree("Chase Tree");
        huntTree = new BehaviorTree("Hunt Tree");

        Sequence patrolSequence = new Sequence("Patrol Sequence");
        Leaf lookAround = new Leaf("Look around", Swivel);

        Selector patrolDestroy = new Selector("Patrol or Destroy");
        Leaf patrol = new Leaf("Go to patrol position", GoToPatrolPoint);
        Leaf destroyItem = new Leaf("Destroy object", DestroyItem);
        patrolDestroy.AddChild(patrol);
        patrolDestroy.AddChild(destroyItem);

        patrolSequence.AddChild(patrolDestroy);
        patrolSequence.AddChild(lookAround);

        chaseTree.AddChild(patrolSequence);

        Selector huntDestroy = new Selector("Hunt or Destroy");
        Leaf hunt = new Leaf("Go to last known character position", HuntCharacter);
        huntDestroy.AddChild(hunt);
        huntDestroy.AddChild(destroyItem); // reuse leaf node from above

 

        Selector lookConsume = new Selector("Look around or Consume character");
        Leaf consume = new Leaf("Consume Character", Consume);
        lookConsume.AddChild(consume);
        lookConsume.AddChild(lookAround); // reuse leaf node from above

        huntDestroy.AddChild(lookConsume);
        huntTree.AddChild(huntDestroy);

        // TODO: figure out when to trigger these states
        // Leaf angry = new Leaf("Angry roar", Angry);
        // Leaf victory = new Leaf("Victory roar", Victory);

        // chaseTree.AddChild(angry);
        // chaseTree.AddChild(victory);

        chaseTree.PrintTree();
        huntTree.PrintTree();

        // TODO: Instantiate Stunned Behavior Tree

        Tree = new BehaviorTree("Base Tree");
        Tree.AddChild(chaseTree);
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

    public Node.Status GoToPatrolPoint()
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
