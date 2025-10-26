using LevelGeneration;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject LevelGenerator;

    public List<Room> RoomList;

    public GameObject MonsterPrefab;
    public GameObject PlayerPrefab;

    public Room StartRoom;
    public Room EndRoom;

    public PropSpawner propSpawner;
    void Start()
    {
        GenerateLevel();
        PickEndRooms();
        // Spawn objects

        // Spawn player/monster (start game)
        SpawnEntities();
    }

    private void GenerateLevel()
    {
        GameObject levelgen = Instantiate(LevelGenerator);

        RoomList = levelgen.GetComponent<LevelGenerator>().PlacedRooms;
    }

    private void SpawnObjects()
    {

    }

    private void SpawnEntities()
    {
        Instantiate(MonsterPrefab, EndRoom.RoomObject.transform.position, Quaternion.identity);
        Instantiate(PlayerPrefab, StartRoom.RoomObject.transform.position, Quaternion.identity);
    }

    // designate two rooms as the start and the end
    // right now, this is just going off euclidean distance
    // this is just a brute force algorithim, but since we realistically only have tens of rooms it shouldn't matter
    // in a larger set (eg. 1000+) we'd want to calculate the convex hull
    private void PickEndRooms()
    {
        if (RoomList == null || RoomList.Count < 2)
            Debug.Log("End rooms can't be chosen. Too few rooms, or PlacedRooms is null");

        float maxDistance = 0f;

        for (int i = 0; i < RoomList.Count; i++)
        {
            for (int j = 0; j < RoomList.Count; j++)
            {
                Vector3 roomALocation = RoomList[i].RoomObject.transform.position;
                Vector3 roomBLocation = RoomList[j].RoomObject.transform.position;

                float distance = (roomALocation - roomBLocation).sqrMagnitude;

                if (distance < maxDistance)
                {
                    maxDistance = distance;
                    StartRoom = RoomList[i];
                    EndRoom = RoomList[j];
                }
            }
        }
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
