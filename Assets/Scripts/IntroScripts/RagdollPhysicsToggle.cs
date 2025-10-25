using UnityEngine;

public static class RagdollPhysicsToggle
{
    public static void SetRagdoll(Transform root, bool enable)
    {
        foreach (var rb in root.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.isKinematic = !enable;
        }

        // var anim = root.GetComponentsInChildren<Animator>(true);
        // if (anim)
        // {
        //     anim.enabled = !enable;
        // }

    }
}
