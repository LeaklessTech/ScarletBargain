using UnityEngine;
using UnityEngine.UI;

using TMPro;

using System.Collections.Generic;
using System.Linq;

public class PreferencesManager : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;

    public Toggle fullScreenToggle;

    private Resolution[] res;
    private List<string> options;

    void Awake()
    {
        PopulateResolutions();
        //LoadPrefsAndApply();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void PopulateResolutions()
    {
        // reference: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Screen-resolutions.html
        res = Screen.resolutions;

        options = res.Select(r => $"{r.width} x {r.height}").ToList();

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();
    }
}
