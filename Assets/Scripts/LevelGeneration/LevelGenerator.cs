using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static LevelGeneration.Tile;

namespace LevelGeneration
{
    public partial class LevelGenerator : MonoBehaviour
    {
        [Header("Step Mode")]
        [Tooltip("Activating this mode makes level generation run at runtime. For debugging purposes.")]
        public bool DebugActive = false;

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
        public int retryLimit = 50;

        [Header("Tile Prefab")]
        public GameObject floorTilePrefab;

        [Tooltip("World-space spacing between tiles (matches tile prefab footprint)")]
        public int objectSizeOffset = 10;

        [SerializeField]
        public int Seed;

        private Tile[,] tileGrid;

        private GameObject LevelObject;

        // For overlap checking
        private readonly List<Room> placedRooms = new();

        private static readonly (Tile.Wall, Vector2Int d)[] Dirs =
        {
            (Wall.North, new Vector2Int(0, 1)),
            (Wall.South, new Vector2Int(0,  -1)),
            (Wall.East,  new Vector2Int(1, 0)),
            (Wall.West,  new Vector2Int(-1,  0)),
        };

        void Start()
        {
            // setting a seed makes debugging easier
            if (Seed == 0)
            {
                System.Random random = new();

                Seed = random.Next(0, int.MaxValue - 1);

                UnityEngine.Random.InitState(Seed);
            }
            else
                UnityEngine.Random.InitState(Seed);

            LevelObject = new("Level");

            tileGrid = new Tile[LevelWidth, LevelLength];

            if (DebugActive)
            {
                StartCoroutine(DebugCoroutine());
            }
            else
            {
                GenerateBase();
                CreateRooms();
                CreateHallways();
            }
        }

        private void RefineRoom(Room room, bool disableInsteadOfDestroy = true)
        {
            if (tileGrid == null) return;

            foreach (var tilePosition in room.Bounds.allPositionsWithin)
            {
                var tile = tileGrid[tilePosition.x, tilePosition.y];
                if (tile == null)
                    continue;
                tile.TileObject.SetActive(true);

                // For each direction, if the neighbor is ALSO inside the rect,
                // remove this tile's wall that faces that neighbor.
                // (When we iterate the neighbor tile later, its opposite wall will also be removed.)
                foreach (var (wall, direction) in Dirs)
                {
                    Vector2Int n = tilePosition + direction;
                    if (room.Bounds.Contains(n))
                    {
                        tile.RemoveWall(wall);
                    }
                }
            }
        }

        private void GenerateBase()
        {
            for (int i = 0; i < LevelWidth; i++)
            {
                for (int j = 0; j < LevelLength; j++)
                {
                    Vector3 createAt = new Vector3(-i * objectSizeOffset, 0, -j * objectSizeOffset);

                    GameObject newTileGameObject = Instantiate(floorTilePrefab, createAt, Quaternion.identity);

                    // debug
                    //newTileGameObject.transform.GetChild(5).GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{i},{j}";

                    Tile newTile = new(Tile.TileType.EMPTY, newTileGameObject, (i, j));

                    tileGrid[i, j] = newTile;

                    // we'll only want rooms to be visible/enabled
                    // unused tiles should be removed in the end (maybe)
                    newTile.TileObject.SetActive(false);

                }
            }
        }

        private void CreateRooms()
        {
            int failedPlacements = 0;

            for (int currentRoom = 0; currentRoom < RoomCount; currentRoom++)
            {
                for (int currentAttempt = 0; currentAttempt < retryLimit; currentAttempt++)
                {
                    int potentialRoomWidth = UnityEngine.Random.Range(MinRoomWidth, MaxRoomWidth + 1);
                    int potentialRoomLength = UnityEngine.Random.Range(MinRoomLength, MaxRoomLength + 1);

                    int potentialRoomX = UnityEngine.Random.Range(0, LevelWidth - potentialRoomWidth + 1);
                    int potentialRoomY = UnityEngine.Random.Range(0, LevelLength - potentialRoomLength + 1);

                    RectInt potentialRoom = new(potentialRoomX, potentialRoomY, potentialRoomWidth, potentialRoomLength);

                    if (BoundsCheck(potentialRoom))
                    {
                        // Collect the tiles/transforms that will belong to this room
                        List<Transform> tileTransforms = new();
                        List<Renderer> tileRenderers = new();
                        List<Collider> tileColliders = new();

                        List<Tile> roomTiles = new();

                        foreach (var position in potentialRoom.allPositionsWithin)
                        {
                            var tile = tileGrid[position.x, position.y];
                            if (tile == null) continue;

                            Transform t = tile.TileObject.transform;
                            tileTransforms.Add(t);

                            // prefer renderer bounds
                            var r = tile.TileObject.GetComponent<Renderer>();
                            if (r != null) tileRenderers.Add(r);

                            var c = tile.TileObject.GetComponent<Collider>();
                            if (c != null) tileColliders.Add(c);

                            // this is important for when we pathfind hallways
                            tile.Type = Tile.TileType.ROOM;

                            roomTiles.Add(tile);
                        }

                        // compute combined center in world space
                        Vector3 combinedCenter;
                        if (tileRenderers.Count > 0)
                        {
                            Bounds combined = tileRenderers[0].bounds;
                            for (int i = 1; i < tileRenderers.Count; i++) combined.Encapsulate(tileRenderers[i].bounds);
                            combinedCenter = combined.center;
                        }
                        else if (tileColliders.Count > 0)
                        {
                            Bounds combined = tileColliders[0].bounds;
                            for (int i = 1; i < tileColliders.Count; i++) combined.Encapsulate(tileColliders[i].bounds);
                            combinedCenter = combined.center;
                        }
                        else if (tileTransforms.Count > 0)
                        {
                            // fallback: average world positions
                            Vector3 sum = Vector3.zero;
                            foreach (var tt in tileTransforms) sum += tt.position;
                            combinedCenter = sum / tileTransforms.Count;
                        }
                        else
                        {
                            combinedCenter = Vector3.zero;
                        }

                        // create the room object at visual center
                        GameObject roomObject = new($"Room {placedRooms.Count + 1}");
                        roomObject.transform.position = combinedCenter;
                        roomObject.transform.parent = LevelObject.transform;

                        // parent tiles to the room object while preserving their world positions
                        foreach (var t in tileTransforms)
                        {
                            t.SetParent(roomObject.transform, true);
                        }

                        placedRooms.Add(new(potentialRoom, roomObject, roomTiles));
                        break;
                    }
                    else if (currentAttempt == retryLimit - 1)
                    {
                        failedPlacements++;
                    }
                }
            }

            foreach (Room room in placedRooms)
            {
                RefineRoom(room);
            }

            Debug.Log($"{RoomCount - failedPlacements}/{RoomCount} Rooms were created.");
        }

        private bool BoundsCheck(RectInt room)
        {
            int matrixWidth = tileGrid.GetLength(0);
            int matrixHeight = tileGrid.GetLength(1);

            bool insideMatrix =
                room.xMin >= 0 &&
                room.yMin >= 0 &&
                room.xMax <= matrixWidth &&
                room.yMax <= matrixHeight;

            if (!insideMatrix) return false;

            bool spacedFromOthers = placedRooms.All(r => !Inflate(r.Bounds, RoomBuffer).Overlaps(room));
            if (!spacedFromOthers) return false;

            return true;
        }

        // lets us modify bounds checking for the buffer without needing to do much rewrite
        private static RectInt Inflate(RectInt r, int n)
        {
            return new RectInt(r.xMin - n, r.yMin - n, r.width + 2 * n, r.height + 2 * n);
        }

        // Regions are bad practice 99% of the time, but this makes collapsing them easier
        #region Coroutines
        // we use the coroutines to slow the generation down at edit time
        // probably need to be later removed

        private IEnumerator DebugCoroutine()
        {
            yield return StartCoroutine(GenerateBaseCoroutine());
            Debug.Log("Base Generated.");
            yield return StartCoroutine(CreateRoomsCoroutine());
            Debug.Log("Grid Generated.");
            yield return StartCoroutine(HallwayCoroutine());
            Debug.Log("Hallways Generated.");
        }

        private IEnumerator GenerateBaseCoroutine()
        {
            for (int i = 0; i < LevelWidth; i++)
            {
                for (int j = 0; j < LevelLength; j++)
                {
                    Vector3 createAt = new Vector3(-i * objectSizeOffset, 0, -j * objectSizeOffset);

                    GameObject newTileGameObject = Instantiate(floorTilePrefab, createAt, Quaternion.identity);

                    // debug
                    newTileGameObject.transform.GetChild(5).GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{i},{j}";

                    Tile newTile = new(Tile.TileType.EMPTY, newTileGameObject, (i, j));

                    tileGrid[i, j] = newTile;

                    // enable the tile so it shows up
                    newTile.TileObject.SetActive(true);
                }
                yield return new WaitForSeconds(.1f);
            }
        }
        private IEnumerator CreateRoomsCoroutine()
        {
            int failedPlacements = 0;

            for (int currentRoom = 0; currentRoom < RoomCount; currentRoom++)
            {
                for (int currentAttempt = 0; currentAttempt < retryLimit; currentAttempt++)
                {
                    int potentialRoomWidth = UnityEngine.Random.Range(MinRoomWidth, MaxRoomWidth + 1);
                    int potentialRoomLength = UnityEngine.Random.Range(MinRoomLength, MaxRoomLength + 1);

                    int potentialRoomX = UnityEngine.Random.Range(0, LevelWidth - potentialRoomWidth + 1);
                    int potentialRoomY = UnityEngine.Random.Range(0, LevelLength - potentialRoomLength + 1);

                    RectInt potentialRoom = new(potentialRoomX, potentialRoomY, potentialRoomWidth, potentialRoomLength);

                    if (BoundsCheck(potentialRoom))
                    {
                        // Collect the tiles/transforms that will belong to this room
                        List<Transform> tileTransforms = new();
                        List<Renderer> tileRenderers = new();
                        List<Collider> tileColliders = new();

                        List<Tile> roomTiles = new();

                        foreach (var position in potentialRoom.allPositionsWithin)
                        {
                            var tile = tileGrid[position.x, position.y];
                            if (tile == null) continue;

                            Transform t = tile.TileObject.transform;
                            tileTransforms.Add(t);

                            // prefer renderer bounds
                            var r = tile.TileObject.GetComponent<Renderer>();
                            if (r != null) tileRenderers.Add(r);

                            var c = tile.TileObject.GetComponent<Collider>();
                            if (c != null) tileColliders.Add(c);

                            // this is important for when we pathfind hallways
                            tile.Type = Tile.TileType.ROOM;

                            roomTiles.Add(tile);
                        }

                        // compute combined center in world space
                        Vector3 combinedCenter;
                        if (tileRenderers.Count > 0)
                        {
                            Bounds combined = tileRenderers[0].bounds;
                            for (int i = 1; i < tileRenderers.Count; i++) combined.Encapsulate(tileRenderers[i].bounds);
                            combinedCenter = combined.center;
                        }
                        else if (tileColliders.Count > 0)
                        {
                            Bounds combined = tileColliders[0].bounds;
                            for (int i = 1; i < tileColliders.Count; i++) combined.Encapsulate(tileColliders[i].bounds);
                            combinedCenter = combined.center;
                        }
                        else if (tileTransforms.Count > 0)
                        {
                            // fallback: average world positions
                            Vector3 sum = Vector3.zero;
                            foreach (var tt in tileTransforms) sum += tt.position;
                            combinedCenter = sum / tileTransforms.Count;
                        }
                        else
                        {
                            combinedCenter = Vector3.zero;
                        }

                        // create the room object at visual center
                        GameObject roomObject = new($"Room {placedRooms.Count + 1}");
                        roomObject.transform.position = combinedCenter;
                        roomObject.transform.parent = LevelObject.transform;

                        // parent tiles to the room object while preserving their world positions
                        foreach (var t in tileTransforms)
                        {
                            t.SetParent(roomObject.transform, true);
                        }

                        placedRooms.Add(new(potentialRoom, roomObject, roomTiles));
                        break;
                    }
                    else if (currentAttempt == retryLimit - 1)
                    {
                        failedPlacements++;
                    }
                }
            }

            foreach (Room room in placedRooms)
            {
                RefineRoom(room);
                yield return new WaitForSeconds(.25f);
            }

            for (int i = 0; i < LevelLength; i++)
            {
                for (int j = 0; j < LevelWidth; j++)
                {
                    if (tileGrid[i, j].Type == TileType.EMPTY)
                        tileGrid[i, j].TileObject.SetActive(false);
                }

                yield return new WaitForSeconds(.1f);
            }

            Debug.Log($"{RoomCount - failedPlacements}/{RoomCount} Rooms were created.");
        }

        #endregion

    }
}