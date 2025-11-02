using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IntroSeat : MonoBehaviour
{
    [Header("Characters (ragdoll roots)")]
    public List<Transform> prisonerRoots = new();

    [Header("Lights to reveal")]
    public List<Light> lights = new();

    [Tooltip("Seconds to let them fall/settle before freezing.")]
    public float settleTime = 2.5f;

    [Tooltip("Seconds to fade lights up after freeze.")]
    public float fadeUpTime = 1.5f;

    [Tooltip("Extra flicker while lights come up.")]
    public bool powerOnFlicker = true;

    float[] _origIntensities;

    void Awake()
    {
        //Debug.Log($"[IntroSeat] timeScale on load = {Time.timeScale}");

        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
        // Capture and zero lights
        _origIntensities = new float[lights.Count];
        for (int i = 0; i < lights.Count; i++)
        {
            _origIntensities[i] = lights[i] ? lights[i].intensity : 0f;
            if (lights[i]) lights[i].intensity = 0f;
        }

        // Start in ragdoll = enabled (bodies fall in darkness)
        foreach (var root in prisonerRoots)
            if (root) RagdollPhysicsToggle.SetRagdoll(root, enable:true);
    }

    void Start() => StartCoroutine(RunSequence());

    IEnumerator RunSequence()
    {
        yield return new WaitForSeconds(settleTime);

        foreach (var root in prisonerRoots)
        {
            if (!root) continue;
            RagdollPhysicsToggle.SetRagdoll(root, enable:false);
            if (!root.GetComponent<AttachTwitch>())
                root.gameObject.AddComponent<AttachTwitch>();
        }

        float t = 0f;
        while (t < fadeUpTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeUpTime);

            for (int i = 0; i < lights.Count; i++)
            {
                var L = lights[i];
                if (!L) continue;

                float baseVal = Mathf.Lerp(0f, _origIntensities[i], k);

                if (powerOnFlicker)
                {
                    float noise = Mathf.PerlinNoise(Time.time * 20f, i * 7.3f);
                    float amp = (1f - k) * 0.5f; // strong at start, fades out
                    baseVal += (noise - 0.5f) * 2f * amp * _origIntensities[i];
                    baseVal = Mathf.Max(0f, baseVal);
                }

                L.intensity = baseVal;
            }

            yield return null;
        }

        for (int i = 0; i < lights.Count; i++)
            if (lights[i]) lights[i].intensity = _origIntensities[i];
    }
}
