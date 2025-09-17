using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace LevelGeneration
{

    public class LevelGenerator : MonoBehaviour
    {
        [Header("Level Size")]
        public int LevelWidth = 10;
        public int LevelLength = 10;

        [Header("Room Parameters")]
        public int MinRoomWidth = 2;
        public int MaxRoomWidth = 5;

        public int MinRoomLength = 2;
        public int MaxRoomLength = 5;

        public int RoomCount = 5;

        [Tooltip("How many failed placement attempts before we give up on adding more rooms")]
        public int retryLimit = 50;

        [Header("Tile Prefab")]
        public GameObject floorTilePrefab;

        [Tooltip("World-space spacing between tiles (matches tile prefab footprint)")]
        public int objectSizeOffset = 10;

        private GameObject[,] floorGrid;
        private bool[,] used; // tiles that belong to rooms

        // For overlap checking
        private readonly List<Room> placedRooms = new();

        void Start()
        {
            // Generate initial grid
            GenerateBase();
            // Randomly create rooms
            CreateRooms();
            // Build Grid
        }
        

        private void GenerateBase()
        {
            for (int i = 0; i < LevelWidth; i++)
            {
                for (int j = 0; j < LevelLength; j++)
                {
                    Vector3 createAt = new Vector3(i * objectSizeOffset, 0, j * objectSizeOffset);

                    GameObject newTile = Instantiate(floorTilePrefab, createAt, Quaternion.identity);

                    floorGrid[i, j] = newTile;

                    // we'll only want rooms to be visible/enabled
                    // unused rooms should be removed in the end (maybe)
                    newTile.SetActive(false);
                }
            }
        }

        private void CreateRooms()
        {
            // this is just here for debugging
            int failedPlacements = 0;

            for (int currentRoom = 0; currentRoom < RoomCount; currentRoom++)
            {
                for (int currentAttempt = 0; currentAttempt < retryLimit; currentAttempt++)
                {
                    // randomly decide room size and position based on parameters
                    int thisRoomWidth = Random.Range(MinRoomWidth, MaxRoomWidth);
                    int thisRoomLength = Random.Range(MinRoomLength, MaxRoomLength);

                    int thisRoomX = Random.Range(0, floorGrid.Length);
                    int thisRoomY = Random.Range(0, floorGrid.Length);

                    RectInt thisRoom = new(thisRoomX, thisRoomY, thisRoomWidth, thisRoomLength);

                    // perform bounds checking, retry up to limit if bounds dont allow
                    if(BoundsCheck(thisRoom))
                    {
                        placedRooms.Add(new(thisRoom));
                        break;
                    }
                    else if (currentAttempt == retryLimit - 1)
                        failedPlacements++;
                }
            }

            Debug.Log($"{RoomCount - failedPlacements}/{RoomCount} Rooms were created.");
        }

        private bool BoundsCheck(RectInt room)
        {
            int matrixWidth = floorGrid.GetLength(0);
            int matrixHeight = floorGrid.GetLength(1);

            // given the way the xMin and yMin are picked via random, this should never be under 0
            bool insideMatrix = room.yMin >= 0 &&
                                room.yMax <= matrixHeight &&
                                room.xMin <= matrixWidth &&
                                room.xMax >= 0;

            if (room.yMin < 0 || room.xMin < 0)
                Debug.LogWarning($"WARNING: Room starting point ({room.xMin},{room.yMin}) was set to less than 0, check LevelGenerator.cs");

            if (!insideMatrix)
                return false;

            bool notIntersectingRoom = placedRooms.All(r => r.BoundsCheck(room));

            if (!notIntersectingRoom)
                return false;

            return true;
        }


    }
}