using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public Light target;
    public float startingIntensity = 20f;
    public float flickerLevel = 3f;
    public float speed = 12f;

    // Update is called once per frame
    void Update()
    {
        if (!target)
        {
            return;
        }
        float n = Mathf.PerlinNoise(Time.time * speed, 0f);
        target.intensity = startingIntensity + (n - 0.5f) * flickerLevel;
    }
}
