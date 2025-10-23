using UnityEngine;

public class FastTwitch : MonoBehaviour
{
    [Header("Twitch Amount & Speed")]
    public float rotationDegrees = 5f;
    public float jitterSpeed    = 8f;
    public float randomOffset   = 100f;

    Vector3 baseEuler;
    float seed;

    void Awake()
    {
        baseEuler = transform.localEulerAngles;
        seed = Random.value * randomOffset;
    }

    void LateUpdate()
    {
        float t  = Time.time * jitterSpeed + seed;
        float rx = (Mathf.PerlinNoise(t,   0f) - 0.5f) * 2f * rotationDegrees;
        float ry = (Mathf.PerlinNoise(0f,  t)  - 0.5f) * 2f * rotationDegrees * 0.6f;
        float rz = (Mathf.PerlinNoise(t,   t)  - 0.5f) * 2f * rotationDegrees * 0.4f;

        transform.localEulerAngles = baseEuler + new Vector3(rx, ry, rz);
    }
}

