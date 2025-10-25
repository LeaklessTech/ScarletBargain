using UnityEngine;

public class LEDBlinker : MonoBehaviour
{
    [Header("Targets")]
    public Renderer[] targets;

    [Header("Color & Intensity")]
    public Color emissionColor = Color.red;
    public float minIntensity = 0.1f;
    public float maxIntensity = 2.5f;

    [Header("Blinking")]
    public float baseSpeed = 3.5f;
    public float desync = 1.0f;
    public float randomPhase = 10f;

    public float glitchChance = 0.07f;
    public float glitchDuration = 0.1f;

    MaterialPropertyBlock mpb;
    int emissionId;
    float[] phase;
    float[] glitchUntil;

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
        emissionId = Shader.PropertyToID("_EmissionColor");
        if (targets == null || targets.Length == 0)
            targets = GetComponentsInChildren<Renderer>();

        phase = new float[targets.Length];
        glitchUntil = new float[targets.Length];
        for (int i = 0; i < targets.Length; i++)
            phase[i] = Random.value * randomPhase;
    }

    void Update()
    {
        float t = Time.unscaledDeltaTime;
        for (int i = 0; i < targets.Length; i++)
        {
            // occasional quick "off" glitches
            if (Time.time > glitchUntil[i] && Random.value < glitchChance * t)
            {
                glitchUntil[i] = Time.time + glitchDuration;
            }

            float speed = baseSpeed + (Random.value - 0.5f) * desync;
            phase[i] += speed * t;

            float ping = Mathf.PingPong(phase[i], 1f);
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, ping);

            if (Time.time < glitchUntil[i])
                intensity = 0f;

            targets[i].GetPropertyBlock(mpb);
            mpb.SetColor(emissionId, emissionColor * intensity);
            targets[i].SetPropertyBlock(mpb);
        }
    }
}

