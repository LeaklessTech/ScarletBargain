using UnityEngine;
using System.Collections;
using TMPro;

public class ExplainOverlay : MonoBehaviour
{
    [SerializeField] GameObject overlay;
    [SerializeField] TMP_Text messageText;

    public float showSeconds = 8f;

    void Start()
    {
        if (overlay == null)
        {
            return;
        }
        overlay.SetActive(true);
        StartCoroutine(ShowAndHide());
    }

    IEnumerator ShowAndHide()
    {
        yield return null;

        UpdateOverlayMessage();

        yield return new WaitForSecondsRealtime(showSeconds);

        overlay.SetActive(false);
    }

    private void UpdateOverlayMessage()
    {
        if (messageText == null && overlay != null)
        {
            messageText = overlay.GetComponentInChildren<TMP_Text>();
        }

        if (messageText == null)
            return;

        GameManager gm = FindObjectOfType<GameManager>();
        if (gm == null)
            return;

        int currentLevel = LevelState.CurrentLevel;
        int totalLevels = LevelState.TotalLevels;
        int prisonersThisLevel = gm.PrisonersThisLevel;

        // messageText.text =
        //     $"There are {totalLevels} levels in this prison, \n " +
        //     $"you are on level {currentLevel}.\n" +
        //     "Find the prisoners \n" +
        //     "and avoid the monster \n" + "\n" +
        //     "\"C\" to crouch and hide \n" +
        //     "\"E\" to have a prisoner follow \n" + "\n" +
        //     "escape via the ladder, \n" +
            
        //     $"See if you can escape with {prisonersThisLevel} prisoners.";
    }

}
