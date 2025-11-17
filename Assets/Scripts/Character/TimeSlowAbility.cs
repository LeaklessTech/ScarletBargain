using System.Collections;
using UnityEngine;


public class TimeSlowAbility : MonoBehaviour
{
    public KeyCode abilityKey = KeyCode.Q;

    public float timeScaleMultiplier = 0.5f;

    // in seconds
    public float abilityDuration = 5f;

    // ability cd
    public float cooldownDuration = 60f;

    public UnityEngine.UI.Image cooldownImage;

    public AudioClip timeSlowSound;
    public AudioSource audioSource;

    public AudioSource runningFootstepAudio;
    private float originalRunningFootstepPitch = 1f;

    // internal
    private bool isAbilityActive;
    private bool isOnCooldown;
    private float originalTimeScale;
    private float originalFixedDeltaTime;
    private float cooldownTimer;

    private void Awake()
    {
        originalTimeScale = Time.timeScale;
        originalFixedDeltaTime = Time.fixedDeltaTime;
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.unscaledDeltaTime;
            if (cooldownImage != null)
            {
                float totalDuration = abilityDuration + cooldownDuration;
                if (totalDuration > 0f)
                {
                    float fill = Mathf.Clamp01(cooldownTimer / totalDuration);
                    cooldownImage.fillAmount = fill;
                }
            }
            if (cooldownTimer <= 0f)
            {
                cooldownTimer = 0f;
            }
        }

        // if the ability is currently active or cooling down, ignore input
        if (isAbilityActive || isOnCooldown) return;

        // listen for the ability key and start the time slow if pressed
        if (Input.GetKeyDown(abilityKey))
        {
            StartCoroutine(ActivateTimeSlow());
        }
    }

    private IEnumerator ActivateTimeSlow()
    {
        isAbilityActive = true;

        cooldownTimer = abilityDuration + cooldownDuration;
        if (cooldownImage != null)
        {
            float totalDuration = abilityDuration + cooldownDuration;
            if (totalDuration > 0f)
                cooldownImage.fillAmount = 1f;
        }

        if (timeSlowSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(timeSlowSound);
        }

        Time.timeScale = timeScaleMultiplier;
        Time.fixedDeltaTime = originalFixedDeltaTime * timeScaleMultiplier;

        if (runningFootstepAudio != null)
        {
            originalRunningFootstepPitch = runningFootstepAudio.pitch;
            runningFootstepAudio.pitch = originalRunningFootstepPitch * timeScaleMultiplier;
        }

        // wait for the duration in real time so it's unaffected by the slowed timescale
        yield return new WaitForSecondsRealtime(abilityDuration);

        // restore og time
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        // restore running footstep pitch
        if (runningFootstepAudio != null)
        {
            runningFootstepAudio.pitch = originalRunningFootstepPitch;
        }

        isAbilityActive = false;

        // begin cd
        if (cooldownDuration > 0f)
        {
            isOnCooldown = true;
            yield return new WaitForSecondsRealtime(cooldownDuration);
            isOnCooldown = false;
        }

        cooldownTimer = 0f;
        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 0f;
        }
    }
}