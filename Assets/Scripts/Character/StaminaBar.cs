using UnityEngine;
using UnityEngine.UI;


// attach to UI GameObject containing an image or slider to represent the fill
public class StaminaBar : MonoBehaviour
{
    public AdvancedPlayerController player;
    public Image fillImage;
    public Slider slider;

    void Start()
    {
        // If no player is explicitly assigned, try to find one in the scene
        if (player == null)
        {
            player = Object.FindFirstObjectByType<AdvancedPlayerController>();
        }

        // configure slider if present
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
        }
    }

    void Update()
    {
        // keeps trying to find player until success
        if (player == null)
        {
            player = Object.FindFirstObjectByType<AdvancedPlayerController>();
            if (player == null)
                return;
        }

        float normalized = player.GetStaminaNormalized();

        // update slider if used
        if (slider != null)
        {
            slider.value = normalized;
        }
        else if (fillImage != null)
        {
            fillImage.fillAmount = normalized;
        }
    }
}