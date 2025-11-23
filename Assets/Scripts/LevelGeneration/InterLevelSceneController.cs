using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class InterlevelSceneController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI theNumber;
    [SerializeField] private TextMeshProUGUI LevelText; 

    [Header("Data")]
    [SerializeField] private FloatVariable prisonerCount;

    private int currentLevel;

    private void Start()
    {
        theNumber.text = prisonerCount.Variable.ToString("0");
        currentLevel = LevelState.CurrentLevel;
        if (LevelText != null)
        {
            LevelText.text = $"Start Level {currentLevel}:";
        }
    }

    public void OnNextLevelButtonPressed()
    {
        SceneManager.LoadScene("LevelGeneration");
    }
}
