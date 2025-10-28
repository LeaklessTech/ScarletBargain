using UnityEngine;

public class DoorTriggerController : MonoBehaviour
{
    private Animator animator;
    private bool playerInRange = false;
    private bool monsterInRange = false;

    void Start()
    {
        // get animator
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // update isOpen
        if(playerInRange || monsterInRange)
        {
            animator.SetBool("isOpen", true);
        }
        else
        {
            animator.SetBool("isOpen", false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // check if triggered
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player entered trigger zone of " + gameObject.name);
        }

        if (other.CompareTag("Monster"))
        {
            monsterInRange = true;
            Debug.Log("Monster entered trigger zone of " + gameObject.name);
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

        if (other.CompareTag("Monster"))
        {
            monsterInRange = false;
            Debug.Log("Monster exited trigger zone of " + gameObject.name);
        }
    }
}