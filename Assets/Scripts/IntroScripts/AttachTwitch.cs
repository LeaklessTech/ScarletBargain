using UnityEngine;
using System.Collections.Generic;

public class AttachTwitch : MonoBehaviour
{
    [Tooltip("If true, add twitch to head/shoulders at runtime.")]
    public bool configureOnStart = true;

    [Header("Head")]
    public float headDeg = 2.0f;
    public float headSpeed = 2.3f;

    [Header("Shoulders")]
    public float shoulderDeg = 1.0f;
    public float shoulderSpeed = 1.8f;

    void Start()
    {
        if (!configureOnStart) return;

        var anim = GetComponentInChildren<Animator>();
        if (!anim || !anim.isHuman)
        {
            return;
        }
        var targets = new List<Transform>
        {
            anim.GetBoneTransform(HumanBodyBones.Head),
            anim.GetBoneTransform(HumanBodyBones.LeftUpperArm),
            anim.GetBoneTransform(HumanBodyBones.RightUpperArm)
        };

        foreach (var t in targets)
        {
            if (!t)
            {
                continue;
            }
            var tw = t.gameObject.AddComponent<FastTwitch>();
            if (t == targets[0])
            {
                tw.rotationDegrees = headDeg;
                tw.jitterSpeed = headSpeed; 
            }
            else
            {
                tw.rotationDegrees = shoulderDeg;
                tw.jitterSpeed = shoulderSpeed; 
            }
        }
    }
}
