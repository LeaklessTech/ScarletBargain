using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PropSpawner propSpawner;
    void Start()
    {
        // Generate level

        // Spawn objects
        // This assumes the level is fully generated
        SpawnObjects();

        // Spawn waypoints

        // Spawn player/monster (start game)
    }

    void SpawnObjects()
    {
        if (propSpawner == null)
        {
            Debug.LogWarning("PropSpawner reference not set.");
            return;
        }

        propSpawner.SpawnNow();
    }
}
