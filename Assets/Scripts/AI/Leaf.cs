using UnityEngine;

public class Leaf : Node
{
    public delegate Status Tick();
    public Tick ProcessMethod;

    public Leaf() { }

    public Leaf(string n, Tick pn)
    {
        name = n;
        ProcessMethod = pn;
    }

    public override Status Process()
    {
        if(ProcessMethod != null)
            return ProcessMethod();
        return Status.FAILURE;
    }
}
