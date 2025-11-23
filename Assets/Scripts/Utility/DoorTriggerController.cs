using UnityEngine;

public class DoorTriggerController : MonoBehaviour
{
    private Animator animator;
    private int agentsInRange = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        animator.SetBool("isOpen", agentsInRange > 0);
    }

    private void CalculateDoorDirection(Transform target)
    {
        Vector3 directionToTarget = target.position - transform.position;
        float dot = Vector3.Dot(transform.forward, directionToTarget);
        animator.SetFloat("DotProduct", dot);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Monster"))
        {
            agentsInRange++;

            CalculateDoorDirection(other.transform);

            Debug.Log($"{other.name} entered. Agents in range: {agentsInRange}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Monster"))
        {
            agentsInRange = Mathf.Max(0, agentsInRange - 1);

            Debug.Log($"{other.name} exited. Agents in range: {agentsInRange}");
        }
    }
}