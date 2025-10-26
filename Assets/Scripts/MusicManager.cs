using UnityEngine;

public class MusicManager : MonoBehaviour
{

    public AudioSource musicSource;
    const string PREF_MUSIC = "pref_music_enabled";

    void Awake()
    {
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        bool enabled = PlayerPrefs.GetInt(PREF_MUSIC, 1) == 1;
        PlayMusic(enabled);
        if (enabled && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void PlayMusic(bool enabled)
    {
        if (musicSource != null)
        {
            musicSource.mute = !enabled;
        }

        PlayerPrefs.SetInt(PREF_MUSIC, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
