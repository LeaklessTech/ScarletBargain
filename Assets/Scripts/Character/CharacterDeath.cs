using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.AI;

public class CharacterDeath : MonoBehaviour
{

    private Rigidbody[] rigidbodyList;

    private AdvancedPlayerController apc;

    public int DisableRagdollTime = 15;

    void Start()
    {
        rigidbodyList = GetComponentsInChildren<Rigidbody>();

        if(this.tag == "Player")
        {
            apc = this.GetComponent<AdvancedPlayerController>();
        }
    }

    public void TriggerDeath(Component sender, object data)
    {
        // Debug.Log("I GOT KILLED. this instance: " + this.gameObject.GetInstanceID() + " and the instance id passed in: " + ((int)data));
        if (data is int)
        {
            int id = (int)data;
            StartCoroutine(DeathSequence(id));
        }
    }
    private IEnumerator DeathSequence(int id)
    {
        if (this.gameObject.tag == "Prisoner" && id == this.gameObject.GetInstanceID())
        {
            if (this.gameObject.GetComponent<PrisonerAI>().isRescued)
            {
                SetRagdoll(true);
                this.gameObject.GetComponent<PrisonerAI>().isDead = true;
                this.gameObject.GetComponent<NavMeshAgent>().enabled = false;
                this.gameObject.GetComponent<RescuablePrisoner>().prisonerCount.Variable.Variable -= 1;
                this.gameObject.layer = 15; // change to dead layer
            }
        }

        if (this.gameObject.tag == "Player" && id == this.gameObject.GetInstanceID())
        {
            apc.SetActive(false);
            SetRagdoll(true);
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene("LoseScene", LoadSceneMode.Single);
        }

    }


    private void SetRagdoll(bool isRagdoll)
    {
        // flip ragdoll bool so that calling SetRagdoll with true sets the character to ragdoll
        GetComponent<Animator>().enabled = !isRagdoll;
        foreach (Rigidbody ragdollBone in rigidbodyList)
        {
            ragdollBone.isKinematic = !isRagdoll;
        }
    }
}


