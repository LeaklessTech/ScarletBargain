using UnityEngine;
using UnityEngine.AI;

public class MonsterBehavior : MonoBehaviour
{
    // Parent Tree
    public BehaviorTree tree;
    public Node.Status treeStatus = Node.Status.RUNNING;

    NavMeshAgent agent;

    // TODO: delete these object references
    public GameObject goal;
    public GameObject secondGoal;

    // Describes whether or not an action is currently active or not, separate from a Node Status
    public enum ActionState { IDLE, WORKING };
    ActionState state = ActionState.IDLE;

    // Specific Behavior Trees
    BehaviorTree chaseTree = new BehaviorTree();
    public Node.Status chaseStatus;

    BehaviorTree patrolTree = new BehaviorTree();
    public Node.Status patrolStatus;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();

        tree = new BehaviorTree();
        Sequence move = new Sequence("Move to objects");
        Leaf goToObject = new Leaf("Go to object", GoToObject);
        Leaf goToOtherGoal = new Leaf("Go to second object", GoToOtherGoal);

        move.AddChild(goToObject);
        move.AddChild(goToOtherGoal);

        tree.AddChild(move);
        tree.PrintTree();
    }

    void Update()
    {
        if (treeStatus != Node.Status.SUCCESS)
            treeStatus = tree.Process();
    }

    #region Behaviors
    public Node.Status GoToObject()
    {
        return GoToLocation(goal.transform.position);
    }

    public Node.Status GoToOtherGoal()
    {
        return GoToLocation(secondGoal.transform.position);
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
