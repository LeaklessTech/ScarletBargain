using UnityEngine;

public class InterLevelCanvasToggle : MonoBehaviour
{
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject howToCanvas;

    private void Awake()
    {
        ShowMain();
    }

    public void ShowHowTo()
    {
        Debug.Log("InterLevelCanvasToggle.ShowHowTo");
        SetCanvasActive(mainCanvas, false);
        SetCanvasActive(howToCanvas, true);
    }

    public void ShowMain()
    {
        Debug.Log("InterLevelCanvasToggle.ShowMain");
        SetCanvasActive(howToCanvas, false);
        SetCanvasActive(mainCanvas, true);
    }

    private void SetCanvasActive(GameObject canvasObject, bool show)
    {
        if (canvasObject == null)
        {
            return;
        }

        CanvasGroup group = canvasObject.GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.alpha = show ? 1f : 0f;
            group.interactable = show;
            group.blocksRaycasts = show;
        }

        canvasObject.SetActive(show);
    }
}
