using UnityEngine;

public class DoorTriggerController : MonoBehaviour
{
    private Animator animator;
    private bool playerInRange = false;

    void Start()
    {
        // get animator
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // update isOpen
        animator.SetBool("isOpen", playerInRange);
    }

    void OnTriggerEnter(Collider other)
    {
        // check if triggered
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player entered trigger zone of " + gameObject.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // check if untriggered
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player exited trigger zone of " + gameObject.name);
        }
    }
}