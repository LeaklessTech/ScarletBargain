using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public class MenuController : MonoBehaviour
{
    public string gameSceneName = "GameScene";
    public string homeSceneName = "IntroScene";
    public GameObject firstSelected;
    private GameObject quitConfirmPanel;

    public GameObject MenuPanel;
    public GameObject SettingsPanel;

    private void OnEnable()
    {
        if (firstSelected != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    public void BackToHome()
    {
        //reference: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/SceneManagement.SceneManager.LoadScene.html
        SceneManager.LoadScene(homeSceneName, LoadSceneMode.Single);
    }

    public void QuitGameImmediate()
    {
        Application.Quit();
    }

    public void OpenPreferences()
    {
        MenuPanel.SetActive(false);
        SettingsPanel.SetActive(true);
    }
    
    public void ClosePreferences()
    {
        SettingsPanel.SetActive(false); 
        MenuPanel.SetActive(true);
    }

    // public void ShowQuitConfirm()
    // {

    // }

    // public void ConfirmQuit()
    // {

    // }

    // public void CancelQuit()
    // {

    // }
}
