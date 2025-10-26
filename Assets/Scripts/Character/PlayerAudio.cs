using Mono.Cecil;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("audio sources")]
    public AudioSource breathingSource;
    public AudioSource footstepSource;
    public AudioSource runSource;

    [Header("audio clips")]
    public AudioClip breathingClip;
    public AudioClip[] footstepClips;
    public AudioClip runLoopClip;

    [Header("footstep timing (seconds)")]
    public float walkStepInterval = 0.46f;
    public float crouchStepInterval = 0.53f;

    [Header("volumes")]
    public float footstepWalkVolume = 0.1f;
    public float footstepCrouchVolume = 0.05f;

    // internal
    private AdvancedPlayerController controller;
    private Rigidbody rb;
    private float stepTimer;

    // for animation Speed param calculation
    private Vector3 _lastRbPos;

    void Awake()
    {
        controller = GetComponent<AdvancedPlayerController>();
        rb = GetComponent<Rigidbody>();

        if (!breathingSource)
            Debug.LogWarning("PlayerAudio: breathingSource is not assigned", this);
        if (!footstepSource)
            Debug.LogWarning("PlayerAudio: footstepSource is not assigned", this);
        if (!runSource)
            Debug.LogWarning("PlayerAudio: runSource is not assigned", this);

        // start breathing
        if (breathingSource && breathingClip)
        {
            breathingSource.clip = breathingClip;
            breathingSource.loop = true;
            breathingSource.Play();
        }

        // config run loop
        if (runSource && runLoopClip)
        {
            runSource.clip = runLoopClip;
            runSource.loop = true;
        }
    }

    private void Start()
    {
        _lastRbPos = rb.position;
    }

    // Update is called once per frame
    void Update()
    {
        // movement state from controller
        bool moving = controller != null ? controller.IsMoving : false;
        bool running = controller != null ? controller.IsRunning : false;
        bool crouching = controller != null ? controller.IsCrouching : false;

        // running loop
        if (runSource && runLoopClip)
        {
            if (running && moving)
            {
                if (!runSource.isPlaying)
                    runSource.Play();
            }
            else
            {
                if (runSource.isPlaying)
                    runSource.Stop();
            }
        }

        // discrete footsteps when moving but not running
        if (footstepSource && moving && !running)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                float interval = crouching ? crouchStepInterval : walkStepInterval;
                AudioClip clip = ChooseRandomClip(footstepClips);
                if (clip)
                {
                    footstepSource.pitch = Random.Range(0.6f, 0.9f);
                    float volumeScale = crouching ? footstepCrouchVolume : footstepWalkVolume;
                    footstepSource.PlayOneShot(clip, volumeScale);
                }
                stepTimer = interval;
            }
        }
        else
        {
            stepTimer = 0.2f;
        }
    }

    private static AudioClip ChooseRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}
