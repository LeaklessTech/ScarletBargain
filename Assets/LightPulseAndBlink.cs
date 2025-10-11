using UnityEngine;
using System.Collections;

public class LightPulseAndBlink : MonoBehaviour
{
    public Light target;
    public float startingIntensity = 15f;
    public float topIntensity = 60f;
    public float pulseTime = 4.0f;
    public int blinkCount = 3;
    public float blinkOnTime = 0.08f;
    public float blinkOffTime = 0.12f;
    public float cooldownTime = 3.0f;
    public bool loopThisEffect = true;

    void OnEnable()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        while (true)
        {
            if (!target)
            {
                yield break;
            }

            float t = 0f;
            float start = startingIntensity;
            while (t < pulseTime)
            {
                t += Time.deltaTime;
                target.intensity = Mathf.Lerp(start, topIntensity, t / pulseTime);
                yield return null;
            }

            for (int i = 0; i < blinkCount; i++)
            {
                target.enabled = false; yield return new WaitForSeconds(blinkOffTime);
                target.enabled = true; yield return new WaitForSeconds(blinkOnTime);
            }

            target.intensity = startingIntensity;
        }

        if (!loopThisEffect) yield break;
        yield return new WaitForSeconds(cooldownTime);

    }
}
