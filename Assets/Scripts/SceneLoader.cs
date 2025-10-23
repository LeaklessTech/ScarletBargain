using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string nextScene = "Game Scene";
    
    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextScene);
    }
}
