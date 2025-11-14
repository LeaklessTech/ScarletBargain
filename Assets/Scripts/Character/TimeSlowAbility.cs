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

    // internal
    private bool isAbilityActive;
    private bool isOnCooldown;
    private float originalTimeScale;
    private float originalFixedDeltaTime;

    private void Awake()
    {
        originalTimeScale = Time.timeScale;
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void Update()
    {
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

        
        Time.timeScale = timeScaleMultiplier;
        Time.fixedDeltaTime = originalFixedDeltaTime * timeScaleMultiplier;

        // wait for the duration in real time so it's unaffected by the slowed timescale
        yield return new WaitForSecondsRealtime(abilityDuration);

        // restore og time
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        isAbilityActive = false;

        // begin cd
        if (cooldownDuration > 0f)
        {
            isOnCooldown = true;
            yield return new WaitForSecondsRealtime(cooldownDuration);
            isOnCooldown = false;
        }
    }
}