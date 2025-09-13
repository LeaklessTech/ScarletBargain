using UnityEngine;
using System.Collections.Generic;

public class Node
{
    public enum Status {SUCCESS, RUNNING, FAILURE};
    public Status status;
    public List<Node> children = new List<Node>();
    public int currentChild = 0;
    public string name;

    public Node() { }

    public Node(string n) {
        name = n;
    }

    public void AddChild(Node n) {
        children.Add(n);
    }
}
