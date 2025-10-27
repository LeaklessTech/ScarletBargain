using LevelGeneration;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public GameObject LevelGenerator;

    public GameObject MonsterPrefab;
    public GameObject PlayerPrefab;
    public GameObject PrisonerPrefab;

    public Room StartRoom;
    public Room EndRoom;

    public PropSpawner propSpawner;

    public LevelGenerator level;

    [Header("Hallway Chance")]
    [Tooltip("Odds that a hallway not in the MST gets added anyway")]
    [Range(0f, 100f)]
    public float AdditionalHallwayChance = 12.5f;

    [Header("Level Size")]
    public int LevelWidth = 10;
    public int LevelLength = 10;

    [Header("Room Parameters")]
    public int MinRoomWidth = 2;
    public int MaxRoomWidth = 5;

    public int MinRoomLength = 2;
    public int MaxRoomLength = 5;

    public int RoomBuffer = 1;

    public int RoomCount = 5;

    [Tooltip("How many failed placement attempts before we give up on adding more rooms")]
    public int RetryLimit = 50;

    [Header("Tile Prefabs")]
    [Tooltip("List of floor tile prefabs to randomly select from for each room")]
    public List<GameObject> FloorTilePrefabs;

    [Tooltip("World-space spacing between tiles (matches tile prefab footprint)")]
    public int ObjectSizeOffset = 10;

    public int Seed;

    public GameObject TilePrefab;

    public GameObject Camera;

    public WaypointListReference WaypointList;

    private bool NavmeshNotGenerated = true;

    void Start()
    {
        GenerateLevel();

        PickEndRooms();

        // Spawn objects
        SpawnObjects();
    }

    private void LateUpdate()
    {
        if(NavmeshNotGenerated)
        {
            CreateNavmesh();
            SpawnEntities();
        }
            
    }

    private void CreateNavmesh()
    {
        GameObject levelObject = GameObject.Find("Level");

        NavMeshSurface surface = (NavMeshSurface)levelObject.AddComponent(typeof(NavMeshSurface));

        surface.BuildNavMesh();

        NavmeshNotGenerated = false;
    }

    private void GenerateLevel()
    {
        // Build a GenerationSettings object using the GameManager's public properties
        GenerationSettings settings = new GenerationSettings
        {
            AdditionalHallwayChance = this.AdditionalHallwayChance,
            LevelWidth = this.LevelWidth,
            LevelLength = this.LevelLength,
            MinRoomWidth = this.MinRoomWidth,
            MaxRoomWidth = this.MaxRoomWidth,
            MinRoomLength = this.MinRoomLength,
            MaxRoomLength = this.MaxRoomLength,
            RoomBuffer = this.RoomBuffer,
            RoomCount = this.RoomCount,
            RetryLimit = this.RetryLimit,
            FloorTilePrefabs = this.FloorTilePrefabs,
            ObjectSizeOffset = this.ObjectSizeOffset,
            Seed = this.Seed,
            TilePrefab = this.TilePrefab,
            WaypointListReference = this.WaypointList
        };

        LevelGenerator generator = new(settings);

        generator.GenerateLevel();
    }

    private void SpawnEntities()
    {
        Instantiate(MonsterPrefab, EndRoom.RoomObject.transform.position, Quaternion.identity);
        GameObject player = Instantiate(PlayerPrefab, StartRoom.RoomObject.transform.position, Quaternion.identity);

        float prisonerSpawnChance = 0.25f;

        int prisonerCount = 0;

        foreach (var room in GlobalVariables.rooms)
        {
            if(Random.Range(0, 1f) < prisonerSpawnChance)
            {
                Instantiate(PrisonerPrefab, room.RoomObject.transform.position, Quaternion.identity);
                prisonerCount++;
            }
        }

        Debug.Log($"{prisonerCount} of {GlobalVariables.rooms.Count} possible prisoners spawned.");
    }

    // designate two rooms as the start and the end
    // right now, this is just going off euclidean distance
    // this is just a brute force algorithim, but since we realistically only have tens of rooms it shouldn't matter
    // in a larger set (eg. 1000+) we'd want to calculate the convex hull
    private void PickEndRooms()
    {
        List<Room> RoomList = GlobalVariables.rooms;

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

                if (distance > maxDistance)
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
