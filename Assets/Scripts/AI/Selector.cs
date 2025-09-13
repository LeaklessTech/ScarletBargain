using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class Selector : Node
{
    public Selector(string n)
    {
        name = n;
    }

    public override Status Process()
    {
        Status childStatus = children[currentChild].Process();

        // If child status is currently running then continue processing that child
        if (childStatus == Status.RUNNING) return Status.RUNNING;

        // If child succeeds then return a success for this selector
        if (childStatus == Status.SUCCESS)
        {
            currentChild = 0;
            return Status.SUCCESS;
        }

        // If current child process failed then proceed to next child
        currentChild++;

        // If all children failed then return a failure for this selector
        if (currentChild >= children.Count)
        {
            currentChild = 0;
            return Status.FAILURE;
        }

        return Status.RUNNING;
    }
}
