using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class InterlevelSceneController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI theNumber;

    [Header("Data")]
    [SerializeField] private FloatVariable prisonerCount;

    [Header("Flow")]
    [SerializeField] private string nextSceneName = "Level2";

    private void Start()
    {
        // Show the prisoner count (no decimals)
        theNumber.text = prisonerCount.Variable.ToString("0");
    }

    public void OnNextLevelButtonPressed()
    {
        SceneManager.LoadScene("LevelGeneration");
    }
}
