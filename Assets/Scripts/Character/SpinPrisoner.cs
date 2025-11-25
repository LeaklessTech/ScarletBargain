using UnityEngine;

public class SpinPrisoner : MonoBehaviour
{
    [Tooltip("Degrees per second around the spin axis.")]
    public float rotationSpeed = 50f;

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }
}
