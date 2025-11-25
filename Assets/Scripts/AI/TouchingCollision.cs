using UnityEngine;

public class TouchingCollision : MonoBehaviour
{
    public GameEvent onCharaceterKilled;

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player" || other.tag == "Prisoner")
        {
            onCharaceterKilled.TriggerEvent(this, other.gameObject.GetInstanceID());
        }
    }
}
