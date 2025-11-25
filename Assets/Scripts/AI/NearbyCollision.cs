using UnityEngine;

public class NearbyCollision : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            // fade in music
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            // fade out music
        }
    }
}
