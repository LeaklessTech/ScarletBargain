using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GiveUp : MonoBehaviour
{
    public string giveUpSceneName = "LoseScene";

    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void AdmitDefeat()
    {
        SceneManager.LoadScene(giveUpSceneName, LoadSceneMode.Single);
    }
}
