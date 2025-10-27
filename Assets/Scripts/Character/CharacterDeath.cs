using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CharacterDeath : MonoBehaviour
{
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
        // just a little delay for the user to take it in
        yield return new WaitForSeconds(1f);

        if (id == this.gameObject.GetInstanceID())
            this.gameObject.SetActive(false);

        if (this.gameObject.tag == "Player")
            SceneManager.LoadScene("LoseScene", LoadSceneMode.Single);

    }
}


