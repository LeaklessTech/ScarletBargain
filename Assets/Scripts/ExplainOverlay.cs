using UnityEngine;
using System.Collections;

public class ExplainOverlay : MonoBehaviour
{
    [SerializeField] GameObject overlay;
    public float showSeconds = 7f;

    void Start()
    {
        if (overlay == null)
        {
            return;
        }
        overlay.SetActive(true);
        StartCoroutine(HideAfterDelay());
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(showSeconds);

        overlay.SetActive(false);
    }
}
