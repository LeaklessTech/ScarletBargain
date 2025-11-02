using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndLadder : MonoBehaviour
{
    public GameObject EscapePromptUI;
    
    private bool playerInRange = false;

    private void Start()
    {
        if (EscapePromptUI != null)
            EscapePromptUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (EscapePromptUI != null)
                EscapePromptUI.SetActive(true);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (EscapePromptUI != null)
                EscapePromptUI.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            SceneManager.LoadScene("WinScene", LoadSceneMode.Single);
        }
    }

}
