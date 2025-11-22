using UnityEngine;

public class SpinPrisoner : MonoBehaviour
{
    [Tooltip("Degrees per second around the spin axis.")]
    public float rotationSpeed = 30f;

    void Update()
    {
        // Rotate around the chosen axis, at rotationSpeed degrees per second
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }
}
