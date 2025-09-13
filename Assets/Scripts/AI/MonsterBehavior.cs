using UnityEngine;

public class MonsterBehavior : MonoBehaviour
{
    BehaviorTree tree;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tree = new BehaviorTree();
        Node steal = new Node("Steal Something");
        Node goToObject = new Node("Go to object");
        Node goToPlayer = new Node("Go to object");

        steal.AddChild(goToObject);
        steal.AddChild(goToPlayer);
        tree.AddChild(steal);

        tree.PrintTree();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
