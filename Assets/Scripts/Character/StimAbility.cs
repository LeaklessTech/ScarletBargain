using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class StimAbility : MonoBehaviour
{
    public KeyCode abilityKey = KeyCode.F;

    public float speedMultiplier = 1.5f;

    public float abilityDuration = 5f;

    public float cooldownDuration = 60f;

    public Image cooldownImage;

    public AudioClip stimSound;
    public AudioSource audioSource;

    // internal
    private bool isAbilityActive;
    private bool isOnCooldown;
    private float cooldownTimer;

    private float originalWalkSpeed;
    private float originalRunSpeed;
    private float originalCrouchSpeed;
    private float originalStaminaDrainRate;

    private AdvancedPlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<AdvancedPlayerController>();
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

        if (isAbilityActive || isOnCooldown) return;

        if (Input.GetKeyDown(abilityKey))
        {
            StartCoroutine(ActivateStim());
        }
    }

    private IEnumerator ActivateStim()
    {
        isAbilityActive = true;

        // set the cooldown timer
        cooldownTimer = abilityDuration + cooldownDuration;
        if (cooldownImage != null)
        {
            float totalDuration = abilityDuration + cooldownDuration;
            if (totalDuration > 0f)
            {
                cooldownImage.fillAmount = 1f;
            }
        }

        // play activation sound
        if (stimSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(stimSound);
        }

        // apply effects
        originalWalkSpeed = playerController.walkSpeed;
        originalRunSpeed = playerController.runSpeed;
        originalCrouchSpeed = playerController.crouchSpeed;
        originalStaminaDrainRate = playerController.staminaDrainRate;

        // apply speed multiplier
        playerController.walkSpeed *= speedMultiplier;
        playerController.runSpeed *= speedMultiplier;
        playerController.crouchSpeed *= speedMultiplier;
        playerController.staminaDrainRate = 0f;

        yield return new WaitForSecondsRealtime(abilityDuration);

        // restore original
        if (playerController != null)
        {
            playerController.walkSpeed = originalWalkSpeed;
            playerController.runSpeed = originalRunSpeed;
            playerController.crouchSpeed = originalCrouchSpeed;
            playerController.staminaDrainRate = originalStaminaDrainRate;
        }

        isAbilityActive = false;

        // begin cooldown
        if (cooldownDuration > 0f)
        {
            isOnCooldown = true;
            yield return new WaitForSecondsRealtime(cooldownDuration);
            isOnCooldown = false;
        }

        // reset cooldown timer
        cooldownTimer = 0f;
        if (cooldownImage != null)
        {
            cooldownImage.fillAmount = 0f;
        }
    }
}