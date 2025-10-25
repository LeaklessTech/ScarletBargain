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
        SyncUIToCurrentScreen();
        LoadPrefsAndApply();
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

    public void OnResolutionChanged(int index)
    {
        if (index < 0 || index >= res.Length)
        {
            return;
        }
        var this_res = res[index];

        //log change
        Debug.Log($"[PreferencesManager] Resolution dropdown changed to {index}: {this_res.width} X {this_res.height}");
        ApplyResolution(this_res.width, this_res.height, Screen.fullScreen);

        PlayerPrefs.SetInt("pref_res_index", index);
        PlayerPrefs.Save();
    }

    void ApplyResolution(int w, int h, bool full)
    {
        Screen.SetResolution(w, h, full);
        //
    }

    public void OnFullScreenToggle(bool isFull)
    {
        ApplyResolution(Screen.width, Screen.height, isFull);

        PlayerPrefs.SetInt("pref_fullscreen", isFull ? 1 : 0);
        PlayerPrefs.Save();
    }

    void SyncUIToCurrentScreen()
    {
        fullScreenToggle.isOn = Screen.fullScreen;

        int currentIndex = 0;
        for (int i = 0; i < res.Length; i++)
        {
            if (res[i].width == Screen.width && res[i].height == Screen.height)
            {
                currentIndex = i;
                break;
            }
        }

        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
    }

    void LoadPrefsAndApply()
    {
        int savedResIndex = PlayerPrefs.GetInt("pref_res_index", GetCurrentResolutionIndex());
        bool savedFull = PlayerPrefs.GetInt("pref_fullscreen", Screen.fullScreen ? 1 : 0) == 1;

        if (savedResIndex < 0 || savedResIndex >= res.Length)
        {
            savedResIndex = GetCurrentResolutionIndex();
        }

        var chosen = res[savedResIndex];
        ApplyResolution(chosen.width, chosen.height, savedFull);

        resolutionDropdown.value = savedResIndex;
        resolutionDropdown.RefreshShownValue();

        fullScreenToggle.isOn = savedFull;
    }

    int GetCurrentResolutionIndex()
    {
        for (int i = 0; i < res.Length; i++)
        {
            if (res[i].width == Screen.width && res[i].height == Screen.height)
            {
                return i;
            }
        }
        return 0;
    }

}
