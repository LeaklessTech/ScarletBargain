using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public GameEvent onPlayerFound;

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
        List<Collider> rangeChecks = Physics.OverlapSphere(transform.position, Radius, TargetMask).ToList();

        if (rangeChecks.Count != 0)
        {
            rangeChecks = rangeChecks.OrderBy(target => Vector3.Distance(target.gameObject.transform.position, transform.position)).ToList();
            TargetRef = rangeChecks.FirstOrDefault().gameObject;

            bool hidden = TargetRef.GetComponent<AdvancedPlayerController>()?.IsCrouching ?? false;
            Transform target = TargetRef.transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < Angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, ObstructionMask) && !hidden)
                {
                    CanSeePlayer = true;
                    onPlayerFound.TriggerEvent(this, new CharacterPosition{ position = target.transform.position, objectId = TargetRef.GetInstanceID() });
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
