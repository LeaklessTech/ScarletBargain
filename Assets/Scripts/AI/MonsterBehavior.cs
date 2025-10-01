using System;
using UnityEngine;
using UnityEngine.AI;

public class MonsterBehavior : MonoBehaviour
{
    // Parent Tree
    public BehaviorTree Tree;
    public Node.Status TreeStatus = Node.Status.RUNNING;

    // Specific Behavior Trees
    BehaviorTree chaseTree;
    BehaviorTree stunnedTree;
    public Node.Status ChaseStatus;
    public Node.Status StunStatus;

    NavMeshAgent agent;

    // Describes whether or not an action is currently active or not, separate from a Node Status
    public enum ActionState { IDLE, WORKING };
    ActionState state = ActionState.IDLE;

    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();

        // Instantiate Chase Tree
        chaseTree = new BehaviorTree();

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

        chaseTree.AddChild(huntDestroy);

        Selector lookConsume = new Selector("Look around or Consume character");
        Leaf consume = new Leaf("Consume Character", Consume);
        lookConsume.AddChild(lookAround); // reuse leaf node from above
        lookConsume.AddChild(consume);

        chaseTree.AddChild(lookConsume);

        Leaf angry = new Leaf("Angry roar", Angry);
        Leaf victory = new Leaf("Victory roar", Victory);

        chaseTree.AddChild(angry);
        chaseTree.AddChild(victory);

        chaseTree.PrintTree();

        // TODO: Instantiate Stunned Behavior Tree

        Tree = new BehaviorTree();
        Tree.AddChild(chaseTree);
    }

    void Update()
    {
        if (TreeStatus != Node.Status.SUCCESS)
            TreeStatus = Tree.Process();
    }

    #region Behaviors
    public Node.Status Swivel()
    {
        throw new NotImplementedException();
    }

    public Node.Status GoToPatrolPoint()
    {
        throw new NotImplementedException();
    }

    public Node.Status DestroyItem()
    {
        throw new NotImplementedException();
    }

    public Node.Status HuntCharacter()
    {
        throw new NotImplementedException();
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
        float distanceToTarget = Vector3.Distance(destination, this.transform.position);

        if (state == ActionState.IDLE)
        {
            agent.SetDestination(destination);
            state = ActionState.WORKING;
        }
        else if (Vector3.Distance(agent.pathEndPosition, destination) >= 2)
        {
            state = ActionState.IDLE;
            return Node.Status.FAILURE;
        }
        else if (distanceToTarget < 2)
        {
            state = ActionState.IDLE;
            return Node.Status.SUCCESS;
        }

        return Node.Status.RUNNING;
    }
    #endregion
}
