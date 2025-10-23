using UnityEngine;

public class Sequence : Node
{
    public Sequence(string n)
    {
        name = n;
    }

    public override Status Process() 
    {
        Status childStatus = children[currentChild].Process();

        // If child status is currently running then continue processing that child
        if (childStatus == Status.RUNNING) return Status.RUNNING;

        // Return failure status if child fails
        if (childStatus == Status.FAILURE) return childStatus;

        // If child status is not FAILURE or RUNNING then it has succeded, proceed to next child process
        currentChild++;

        // If all children completed then return success for this sequence
        if (currentChild >= children.Count)
        {
            currentChild = 0;
            return Status.SUCCESS;
        }

        return Status.RUNNING;
    }
}
