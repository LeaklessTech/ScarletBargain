using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.UIElements;

public class Captcha : MonoBehaviour
{

    System.Random rnd = new System.Random();
    public Toggle[] ToggleList;

    private void Awake()
    {
        var invalidImages = Resources.LoadAll<Texture>("Images/Captcha/InvalidImages");
        var validImages = Resources.LoadAll<Texture>("Images/Captcha/ValidImages");

        ToggleList = this.transform.Find("Panel").gameObject.GetComponentsInChildren<Toggle>();

        RawImage image1 = transform.Find("Panel/Image1").gameObject.GetComponent<RawImage>();
        image1.texture = Resources.Load<Texture>("Images/Captcha/InvalidImages/bicycle_0");


        for(int i = 0; i < ToggleList.Length; i++)
        {
            int index = i;
            ToggleList[index].onValueChanged.AddListener(delegate{ HighlightToggle(ToggleList[index]); });
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

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
