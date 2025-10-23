using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
// using UnityEngine.UIElements;

public class Captcha : MonoBehaviour
{

    System.Random rnd = new System.Random();
    private void Awake()
    {
        var invalidImages = Resources.LoadAll<Texture>("Images/Captcha/InvalidImages");
        var validImages = Resources.LoadAll<Texture>("Images/Captcha/ValidImages");

        var toggles = this.transform.Find("Panel").gameObject.GetComponentsInChildren<UnityEngine.UI.Toggle>();

        RawImage image1 = transform.Find("Panel/Image1").gameObject.GetComponent<RawImage>();
        image1.texture = Resources.Load<Texture>("Images/Captcha/InvalidImages/bicycle_0");


        toggles.FirstOrDefault().onValueChanged.AddListener(delegate { f(); });
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    private void f()
    {
        
    }
}
