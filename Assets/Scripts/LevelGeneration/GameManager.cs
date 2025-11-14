using LevelGeneration;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public GameObject MonsterPrefab;
    public GameObject PlayerPrefab;
    public GameObject PrisonerPrefab;

    public GameObject LadderPrefab;

    public Room StartRoom;
    public Room EndRoom;

    public PropSpawner propSpawner;

    public LevelGenerator level;

    [Header("Prisoner Chance")]
    [Range(0f, 100f)]
    public float PrisonerChance = 12.5f;

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

    [Header("Lighting")]
    [Tooltip("List of light prefabs to randomly select from.")]
    public List<GameObject> LightPrefabs = new List<GameObject>();

    [Header("Doorways")]
    [Tooltip("Doorway prefabs in the same order as FloorTilePrefabs (e.g. Tile 3 to Doorway 3)")]
    public List<GameObject> DoorwayPrefabs = new List<GameObject>();

    public WaypointListReference WaypointList;

    public FloatReference prisonerCount;

    private bool NavmeshNotGenerated = true;

    void Start()
    {
        prisonerCount.Variable.Variable = 0;

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
            SpawnLadder();
        }
            
    }

    private void SpawnLadder()
    {
        Instantiate(LadderPrefab, EndRoom.RoomObject.transform.position + new Vector3(0,2,0), Quaternion.identity);
    }

    private void CreateNavmesh()
    {
        GameObject levelObject = GameObject.Find("Level");

        if (levelObject == null) return;

        NavMeshSurface surface = levelObject.AddComponent<NavMeshSurface>();

        int doorLayer = 3;

        surface.layerMask &= ~(1 << doorLayer);

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
            LightPrefabs = this.LightPrefabs,
            DoorwayPrefabs = this.DoorwayPrefabs,
            WaypointListReference = this.WaypointList
        };

        level = new LevelGenerator(settings); // Store reference if needed

        level.GenerateLevel();
    }

    private void SpawnEntities()
    {
        if (EndRoom?.RoomObject == null) return;
        Instantiate(MonsterPrefab, EndRoom.RoomObject.transform.position, Quaternion.identity);
        if (StartRoom?.RoomObject == null) return;
        GameObject player = Instantiate(PlayerPrefab, StartRoom.RoomObject.transform.position, Quaternion.identity);
        
        if (player.GetComponent<TimeSlowAbility>() == null)
        {
            player.AddComponent<TimeSlowAbility>();
        }

        int prisonerCount = 0;

        foreach (var room in GlobalVariables.rooms)
        {
            if (UnityEngine.Random.Range(0f, 1f) < PrisonerChance)
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
        List<Room> roomList = GlobalVariables.rooms;

        if (roomList == null || roomList.Count < 2)
        {
            Debug.Log("End rooms can't be chosen. Too few rooms, or PlacedRooms is null");
            return;
        }

        float maxDistance = 0f;

        Room tempStart = null;
        Room tempEnd = null;

        for (int i = 0; i < roomList.Count; i++)
        {

            for (int j = 0; j < roomList.Count; j++)
            {

                if (i == j) continue;

                Vector3 roomALocation = roomList[i].RoomObject.transform.position;

                Vector3 roomBLocation = roomList[j].RoomObject.transform.position;

                float distance = (roomALocation - roomBLocation).sqrMagnitude;

                if (distance > maxDistance)
                {

                    maxDistance = distance;

                    tempStart = roomList[i];

                    tempEnd = roomList[j];

                }

            }

        }


        StartRoom = tempStart;
        EndRoom = tempEnd;
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