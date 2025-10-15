using System;
using System.Collections;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float Radius;
    [Range(0, 360)]
    public float Angle;

    public GameObject TargetRef;

    public LayerMask TargetMask;
    public LayerMask ObstructionMask;

    public bool CanSeePlayer;
    // Default to true for now
    public bool SearchForPlayer = true;
    public float Delay = 0.2f;

    // When player is found send an alert to all listeners
    public static event Action<Vector3> OnPlayerFound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // characterRefs = GameObject.FindGameObjectsWithTag("character").ToList();
        StartCoroutine(FOVRoutine());
    }

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(Delay);

        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, Radius, TargetMask);

        if (rangeChecks.Length != 0)
        {
            for (int i = 0; i <= rangeChecks.Length; i++)
            {
                TargetRef = rangeChecks[i].gameObject;
                break;
            }

            Transform target = TargetRef.transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < Angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, ObstructionMask))
                {
                    CanSeePlayer = true;
                    OnPlayerFound?.Invoke(target.transform.position);
                }
                else
                {
                    CanSeePlayer = false;
                    TargetRef = null;
                }
            }
            else
            {
                CanSeePlayer = false;
                TargetRef = null;
            }
        }
        else // Can no longer see target, so set to false
            if (CanSeePlayer)
            {
                CanSeePlayer = false;
                TargetRef = null;
            }
    }

}
