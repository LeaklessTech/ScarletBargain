using System;
using System.Collections;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float radius;
    [Range(0, 360)]
    public float angle;

    public GameObject targetRef;

    public LayerMask targetMask;
    public LayerMask obstructionMask;

    public bool canSeePlayer;
    // Default to true for now
    public bool searchForPlayer = true;
    public float delay = 0.2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // characterRefs = GameObject.FindGameObjectsWithTag("character").ToList();
    }

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(delay);

        while (true)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if (rangeChecks.Length != 0)
        {
            for (int i = 0; i <= rangeChecks.Length; i++)
            {
                targetRef = rangeChecks[i].gameObject;
                break;
            }

            Transform target = targetRef.transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    canSeePlayer = true;
                else
                {
                    canSeePlayer = false;
                    targetRef = null;
                }
            }
            else
            {
                canSeePlayer = false;
                targetRef = null;
            }
        }
        else // Can no longer see target, so set to false
            if (canSeePlayer)
            {
                canSeePlayer = false;
                targetRef = null;
            }
    }

}
