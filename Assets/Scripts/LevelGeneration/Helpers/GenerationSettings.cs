using System;
using System.Collections.Generic;
using UnityEngine;

public class GenerationSettings
{
    private float additionalHallwayChance = 12.5f;
    public float AdditionalHallwayChance
    {
        get => additionalHallwayChance;
        set => additionalHallwayChance = value;
    }

    private int levelWidth = 10;
    public int LevelWidth
    {
        get => levelWidth;
        set => levelWidth = value;
    }

    private int levelLength = 10;
    public int LevelLength
    {
        get => levelLength;
        set => levelLength = value;
    }

    private int minRoomWidth = 2;
    public int MinRoomWidth
    {
        get => minRoomWidth;
        set => minRoomWidth = value;
    }

    private int maxRoomWidth = 5;
    public int MaxRoomWidth
    {
        get => maxRoomWidth;
        set => maxRoomWidth = value;
    }

    private int minRoomLength = 2;
    public int MinRoomLength
    {
        get => minRoomLength;
        set => minRoomLength = value;
    }

    private int maxRoomLength = 5;
    public int MaxRoomLength
    {
        get => maxRoomLength;
        set => maxRoomLength = value;
    }

    private int roomBuffer = 1;
    public int RoomBuffer
    {
        get => roomBuffer;
        set => roomBuffer = value;
    }

    private int roomCount = 5;
    public int RoomCount
    {
        get => roomCount;
        set => roomCount = value;
    }

    private int retryLimit = 50;
    public int RetryLimit
    {
        get => retryLimit;
        set => retryLimit = value;
    }

    private List<GameObject> floorTilePrefabs = new List<GameObject>();
    public List<GameObject> FloorTilePrefabs
    {
        get => floorTilePrefabs;
        set => floorTilePrefabs = value ?? new List<GameObject>();
    }

    private int objectSizeOffset = 10;
    public int ObjectSizeOffset
    {
        get => objectSizeOffset;
        set => objectSizeOffset = value;
    }

    private int seed;
    public int Seed
    {
        get => seed;
        set => seed = value;
    }

    private GameObject tilePrefab;
    public GameObject TilePrefab
    {
        get => tilePrefab;
        set => tilePrefab = value;
    }

    private WaypointListReference waypointListReference;
    public WaypointListReference WaypointListReference
    {
        get => waypointListReference;
        set => waypointListReference = value;
    }


    private List<GameObject> lightPrefabs = new List<GameObject>();

    public List<GameObject> LightPrefabs

    {

        get => lightPrefabs;

        set => lightPrefabs = value ?? new List<GameObject>();

    }

    private List<GameObject> doorwayPrefabs = new List<GameObject>();

    public List<GameObject> DoorwayPrefabs

    {

        get => doorwayPrefabs;

        set => doorwayPrefabs = value ?? new List<GameObject>();

    }
}
