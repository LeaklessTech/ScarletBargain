using UnityEngine;
using System.Collections.Generic;

public class BehaviorTree : Node
{
    readonly IPolicy policy;
    
    public BehaviorTree(string n, IPolicy policy = null) : base(n) {
        this.policy = policy ?? Policies.RunForever;
    }

    public override Status Process()
    {
        Status status = children[currentChild].Process();
        if (policy.ShouldReturn(status)) {
            return status;
        }
        
        currentChild = (currentChild + 1) % children.Count;
        return Status.RUNNING;
    }

    struct NodeLevel 
    {
        public int level;
        public Node node;
    }

    public void PrintTree() 
    {
        string treePrintout = "";
        Stack<NodeLevel> nodeStack = new Stack<NodeLevel>();
        Node currentNode = this;
        nodeStack.Push(new NodeLevel {level = 0, node = currentNode });
        
        while (nodeStack.Count != 0)
        {
            NodeLevel nextNode = nodeStack.Pop();
            treePrintout += new string ('-', nextNode.level) + nextNode.node.name + "\n";
            for(int i = nextNode.node.children.Count -1; i >=0; i--)
            {
                nodeStack.Push(new NodeLevel { level = nextNode.level + 1, node = nextNode.node.children[i] });
            }
        }

        Debug.Log(treePrintout);
    }
}
