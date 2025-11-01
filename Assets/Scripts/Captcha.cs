using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.UIElements;

public class Captcha : MonoBehaviour
{

    System.Random rnd = new System.Random();
    private Toggle[] toggleList;

    private bool correctImageSelected = false;
    private int correctImageIndex;

        // Event Listeners
    [Header("Events")]
    public GameEvent onCaptchaSubmit;

    private void Awake()
    {
        var invalidImages = Resources.LoadAll<Texture>("Images/Captcha/InvalidImages");
        var validImages = Resources.LoadAll<Texture>("Images/Captcha/ValidImages");

        toggleList = this.transform.Find("Panel").gameObject.GetComponentsInChildren<Toggle>();

        correctImageIndex = rnd.Next(0, toggleList.Length);


        for (int i = 0; i < toggleList.Length; i++)
        {
            int index = i;
            toggleList[index].onValueChanged.AddListener(delegate { HighlightToggle(toggleList[index]); });

            if (index == correctImageIndex)
            {
                toggleList[index].GetComponentInChildren<RawImage>().texture = validImages[rnd.Next(0, validImages.Length)];
            }
            else
            {
                toggleList[index].GetComponentInChildren<RawImage>().texture = invalidImages[rnd.Next(0, invalidImages.Length)];
            }
        }

        var submitButton = this.transform.Find("Panel").GetComponentInChildren<Button>();
        submitButton.onClick.AddListener(delegate { SubmitClick(); });
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    // When clicking submit send bool of whether or not the attempt was successful
    private void SubmitClick()
    {
        bool invalidImagesSelected = false;
        for (int i = 0; i < toggleList.Length; i++)
        {
            if (toggleList[i].isOn && i != correctImageIndex)
            {
                invalidImagesSelected = true;
            }
        }

        if (!invalidImagesSelected && toggleList[correctImageIndex].isOn)
            correctImageSelected = true;
        else
            correctImageSelected = false;


        onCaptchaSubmit.TriggerEvent(this, correctImageSelected);
        Debug.Log("Captcha was successful? " + correctImageSelected);
    }

    private void HighlightToggle(Toggle toggle)
    {
        if (toggle.isOn)
        {
            var l = toggle.colors;
            l.normalColor = Color.blue;
            l.selectedColor = Color.blue;
            l.highlightedColor = Color.blue;
            l.pressedColor = Color.blue;

            toggle.colors = l;
        }
        else
        {
            var l = toggle.colors;
            l.normalColor = Color.white;
            l.selectedColor = Color.white;
            l.highlightedColor = Color.white;
            l.pressedColor = Color.white;

            toggle.colors = l;
        }
    }
}
