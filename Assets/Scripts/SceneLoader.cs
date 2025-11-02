using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string nextScene = "Game Scene";

    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextScene);
    }

    public void LoadGame()
    {
        nextScene = "GameScene";
        LoadNextScene();
    }
    
    public void LoadPreferences()
    {
        nextScene = "PreferencesScene";
        LoadNextScene();
    }
}
