using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace LevelGeneration
{
    public partial class LevelGenerator : MonoBehaviour
    {
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
        public int retryLimit = 50;

        [Header("Tile Prefab")]
        public GameObject floorTilePrefab;

        [Tooltip("World-space spacing between tiles (matches tile prefab footprint)")]
        public int objectSizeOffset = 10;

        private GameObject[,] floorGrid;

        private GameObject LevelObject;
        
        // For overlap checking
        private readonly List<Room> placedRooms = new();

        private static readonly (string wall, Vector2Int d)[] Dirs =
        {
            ("NorthWall", new Vector2Int(0, -1)),
            ("SouthWall", new Vector2Int(0,  1)),
            ("EastWall",  new Vector2Int(-1, 0)),
            ("WestWall",  new Vector2Int(1,  0)),
        };

        void Start()
        {
            LevelObject = new("Level");

            floorGrid = new GameObject[LevelWidth, LevelLength];
            // Generate initial grid
            GenerateBase();
            // Randomly create rooms
            CreateRooms();
            // Build Grid
            GenerateGrid();
            // Create connections
            //GenerateHallways();

            //foreach (var veccy in vec)
            //{
            //    Debug.Log(veccy);
            //}    
        }

        public List<Vector3> vec;

        private void Update()
        {
            List<Vector3> veccy = CreateBWDelaunay(LevelObject);

            Debug.DrawLine(veccy[0], veccy[1], Color.red, 0.01f);
            Debug.DrawLine(veccy[1], veccy[2], Color.red, 0.01f);
            Debug.DrawLine(veccy[2], veccy[0], Color.red, 0.01f);
        }


        private void GenerateGrid()
        {
            foreach (Room room in placedRooms)
            {
                RefineRoom(room);
            }
        }

        private void RefineRoom(Room room, bool disableInsteadOfDestroy = true)
        {
            if (floorGrid == null) return;

            foreach (var tilePosition in room.bounds.allPositionsWithin)
            {
                var tile = floorGrid[tilePosition.x, tilePosition.y];
                if (tile == null) 
                    continue;
                tile.SetActive(true);


                // For each direction, if the neighbor is ALSO inside the rect,
                // remove this tile's wall that faces that neighbor.
                // (When we iterate the neighbor tile later, its opposite wall will also be removed.)
                foreach (var (wall, direction) in Dirs)
                {
                    Vector2Int n = tilePosition + direction;
                    if (room.bounds.Contains(n))
                    {
                        RemoveWall(tile, wall, disableInsteadOfDestroy);
                    }
                }
            }
        }

        private static void RemoveWall(GameObject parent, string childName, bool disable)
        {
            var t = parent.transform.Find(childName);
            if (t == null) return;

            if (disable)
                t.gameObject.SetActive(false);
            else
                Object.Destroy(t.gameObject);
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
                    int potentialRoomWidth = Random.Range(MinRoomWidth, MaxRoomWidth + 1);
                    int potentialRoomLength = Random.Range(MinRoomLength, MaxRoomLength + 1);


                    int potentialRoomX = Random.Range(0, LevelWidth - potentialRoomWidth + 1);
                    int potentialRoomY = Random.Range(0, LevelLength - potentialRoomLength + 1);

                    RectInt potentialRoom = new(potentialRoomX, potentialRoomY, potentialRoomWidth, potentialRoomLength);

                    // perform bounds checking, retry up to limit if bounds dont allow
                    if (BoundsCheck(potentialRoom))
                    {
                        GameObject roomObject = new($"Room {placedRooms.Count + 1}");

                        foreach (var position in potentialRoom.allPositionsWithin)
                        {
                            floorGrid[position.x, position.y].transform.parent = roomObject.transform;
                            roomObject.transform.parent = LevelObject.transform;
                        }

                        placedRooms.Add(new(potentialRoom, roomObject));
                       
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

            bool insideMatrix =
                room.xMin >= 0 &&
                room.yMin >= 0 &&
                room.xMax <= matrixWidth &&
                room.yMax <= matrixHeight;

            if (!insideMatrix) return false;

            bool spacedFromOthers = placedRooms.All(r => !Inflate(r.bounds, RoomBuffer).Overlaps(room));
            if (!spacedFromOthers) return false;

            return true;
        }

        // lets us modify bounds checking for the buffer without needing to do much rewrite
        private static RectInt Inflate(RectInt r, int n)
        {
            return new RectInt(r.xMin - n, r.yMin - n, r.width + 2 * n, r.height + 2 * n);
        }

    }
}