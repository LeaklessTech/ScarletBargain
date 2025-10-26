using UnityEngine;
using UnityEngine.UI;

using TMPro;

using System.Collections.Generic;
using System.Linq;

public class PreferencesManager : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;

    public Toggle fullScreenToggle;
    public Toggle musicToggle;

    private Resolution[] res;
    private List<string> options;

    const string PREF_MUSIC = "pref_music_enabled";

    void Awake()
    {
        EnsurePrefsDefaults();

        if (resolutionDropdown == null || fullScreenToggle == null)
        {
            Debug.LogWarning("[PreferencesManager] UI refs not set. Skipping init.");
            return;
        }

        PopulateResolutions();
        SyncUIToCurrentScreen();
        LoadPrefsIntoUI();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ApplyCurrentUISettings();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnEnable()
    {
        LoadPrefsIntoUI();
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

        Debug.Log($"[PreferencesManager] Fullscreen toggle selected.");


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

    void LoadPrefsIntoUI()
    {
        int savedResIndex = PlayerPrefs.GetInt("pref_res_index", GetCurrentResolutionIndex());
        bool savedFull = PlayerPrefs.GetInt("pref_fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        bool savedMusic = PlayerPrefs.GetInt(PREF_MUSIC, 1) == 1;

        if (savedResIndex < 0 || savedResIndex >= res.Length)
        {
            savedResIndex = GetCurrentResolutionIndex();
        }

        resolutionDropdown.value = savedResIndex;
        resolutionDropdown.RefreshShownValue();
        if (fullScreenToggle != null)
        {
            fullScreenToggle.isOn = savedFull;

        }

        if (musicToggle != null)
        {
            musicToggle.SetIsOnWithoutNotify(savedMusic);
        }
    }

    void ApplyCurrentUISettings()
    {
        int index = resolutionDropdown.value;

        if (index < 0 || index >= res.Length)
        {
            index = GetCurrentResolutionIndex();
        }

        Resolution chosen = res[index];
        bool fullscreen = fullScreenToggle.isOn;

        ApplyResolution(chosen.width, chosen.height, fullscreen);

        PlayerPrefs.SetInt("pref_res_index", index);
        PlayerPrefs.SetInt("pref_fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.Save();

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

    public void OnMusicToggle(bool enabled)
    {
        PlayerPrefs.SetInt(PREF_MUSIC, enabled ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"[Preferences] Music state is {enabled}.");
    }

    // when I created the preferences for music on/off
    // the music stopped working.  I'm adding this to 
    // ensure the music defaults to on.

    void EnsurePrefsDefaults()
    {
        if (!PlayerPrefs.HasKey(PREF_MUSIC))
        {
            PlayerPrefs.SetInt(PREF_MUSIC, 1);   // default ON
            PlayerPrefs.Save();
            Debug.Log("[Preferences] Initialized default music=ON");
        }
    }
}
