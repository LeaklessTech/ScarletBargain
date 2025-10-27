using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterDeath : MonoBehaviour
{
    public void TriggerDeath(Component sender, object data)
    {
        // Debug.Log("I GOT KILLED. this instance: " + this.gameObject.GetInstanceID() + " and the instance id passed in: " + ((int)data));
        if(data is int)
        {
            int id = (int)data;

            if (id == this.gameObject.GetInstanceID())
                this.gameObject.SetActive(false);

            if (this.gameObject.tag == "Player")
                SceneManager.LoadScene("LoseScene", LoadSceneMode.Single);
        }
    }
}
