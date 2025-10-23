using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public class MenuController : MonoBehaviour
{
    public string gameSceneName = "GameScene";
    public GameObject firstSelected;
    private GameObject quitConfirmPanel;

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

    public void QuitGameImmediate()
    {
        Application.Quit();
    }

    public void PreferencesScene()
    {
        SceneManager.LoadScene("PreferenceScene", LoadSceneMode.Single);
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
