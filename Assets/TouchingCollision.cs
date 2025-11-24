using UnityEngine;

public class TouchingCollision : MonoBehaviour
{


    public GameEvent onCharaceterKilled;

    private int huntedPlayerId = -1;

    void OnTriggerEnter(Collider other)
    {
        onCharaceterKilled.TriggerEvent(this, huntedPlayerId);
        huntedPlayerId = -1;
    }


    public void TriggerHunt(Component sender, object data)
    {
        if (data is CharacterPosition)
        {
            huntedPlayerId = ((CharacterPosition)data).objectId;
        }
    }
}
