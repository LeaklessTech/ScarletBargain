using UnityEngine;
using UnityEngine.Audio;


//Notes:
// the clips used for the ambient sound came from:
// https://freesound.org/
// and all were filtered to have 
// creative commons licensing.  

public class RandomAmbientNoise : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Sound Assets")]
    public AudioClip[] soundClips;

    [Header("Delay Range, probability of sound and max sounds")]
    public Vector2 delayRange = new Vector2(3f, 20f);
    public float chancePerEvent = 0.9f;
    public int maxSimultaneousSounds = 1;
    public Vector2 volumeRange = new Vector2(0.5f, 0.9f);

    private int currentlyPlaying = 0;

    float timer;
    int previousIndex = -1;


    void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetTimer();
    }

    // Update is called once per frame
    void Update()
    {
        if (soundClips == null || soundClips.Length == 0)
        {
            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0f)
        {
            return;
        }

        ResetTimer();

        if (Random.value > chancePerEvent)
        {
            return;
        }

        if (currentlyPlaying >= Mathf.Max(1, maxSimultaneousSounds))
        {
            return;
        }

        int index = GetRandomSoundIndex();
        var clip = soundClips[index];

        previousIndex = index;

        var volume = Random.Range(volumeRange.x, volumeRange.y);

        // reference https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AudioSource.PlayOneShot.html
        audioSource.PlayOneShot(clip, volume);

        currentlyPlaying++;
        float duration = clip.length;
        Invoke(nameof(DecrementPlaying), duration);
    }

    void DecrementPlaying()
    {
        currentlyPlaying = (Mathf.Max(0, currentlyPlaying - 1));

    }

    int GetRandomSoundIndex()
    {
        int this_index;
        this_index = Random.Range(0, soundClips.Length);

        return this_index;
    }

    void ResetTimer()
    {
        timer = Random.Range(delayRange.x, delayRange.y);
    }
}
