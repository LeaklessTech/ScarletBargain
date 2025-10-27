using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static LevelGeneration.Tile;

namespace LevelGeneration
{
    public partial class LevelGenerator
    {
        public GenerationSettings Settings { get; private set; }

        // Backwards-compatible property accessors that read from Settings.
        // These let existing code continue to reference simple names (e.g. LevelWidth)
        // while the actual values are stored in GenerationSettings.
        private float AdditionalHallwayChance => Settings != null ? Settings.AdditionalHallwayChance : 12.5f;
        private int LevelWidth => Settings != null ? Settings.LevelWidth : 10;
        private int LevelLength => Settings != null ? Settings.LevelLength : 10;
        private int MinRoomWidth => Settings != null ? Settings.MinRoomWidth : 2;
        private int MaxRoomWidth => Settings != null ? Settings.MaxRoomWidth : 5;
        private int MinRoomLength => Settings != null ? Settings.MinRoomLength : 2;
        private int MaxRoomLength => Settings != null ? Settings.MaxRoomLength : 5;
        private int RoomBuffer => Settings != null ? Settings.RoomBuffer : 1;
        private int RoomCount => Settings != null ? Settings.RoomCount : 5;
        private int RetryLimit => Settings != null ? Settings.RetryLimit : 50;
        private List<GameObject> FloorTilePrefabs => Settings != null ? Settings.FloorTilePrefabs : new List<GameObject>();
        private int ObjectSizeOffset => Settings != null ? Settings.ObjectSizeOffset : 10;
        private int Seed => Settings != null ? Settings.Seed : 0;
        private GameObject TilePrefab => Settings != null ? Settings.TilePrefab : null;
        private WaypointListReference WaypointList => Settings != null ? Settings.WaypointListReference : null;

        private Tile[,] tileGrid;

        private GameObject _levelObject;

        private GameObject _hallwayObject;

        // For overlap checking
        private readonly List<Room> _placedRooms = new();

        private static readonly (Tile.Wall, Vector2Int d)[] Dirs =
        {
            (Wall.North, new Vector2Int(0, 1)),
            (Wall.South, new Vector2Int(0,  -1)),
            (Wall.East,  new Vector2Int(1, 0)),
            (Wall.West,  new Vector2Int(-1,  0)),
        };

        public LevelGenerator(GenerationSettings settings)
        {
            Settings = settings;
        }

        public void GenerateLevel()
        {
            // setting a seed makes debugging easier
            int seedValue = Settings != null ? Settings.Seed : 0;

            if (seedValue == 0)
            {
                System.Random random = new();

                seedValue = random.Next(0, int.MaxValue - 1);

                if (Settings != null)
                    Settings.Seed = seedValue;

                UnityEngine.Random.InitState(seedValue);
            }
            else
            {
                UnityEngine.Random.InitState(seedValue);
            }

            _levelObject = new("Level");

            tileGrid = new Tile[LevelWidth, LevelLength];

            //WaypointList.WaypointListRef.Clear();

            GenerateTileGrid();
            SimulateRooms();
            CreateTiles();
            RefineRooms();
            CreateHallways();
            RemoveRemainingTiles();

            GlobalVariables.rooms = _placedRooms;
        }

        // clean up unused tiles
        private void RemoveRemainingTiles()
        {
            _hallwayObject = new("Hallways");

            for (int i = 0; i < LevelWidth; i++)
            {
                for (int j = 0; j < LevelLength; j++)
                {
                    Tile currentTile = tileGrid[i, j];

                    if (currentTile.Type == TileType.EMPTY)
                    {
                        GameObject.Destroy(currentTile.TileObject);
                        tileGrid[i, j] = null;
                    }

                    if (currentTile.Type == TileType.HALLWAY)
                        currentTile.TileObject.transform.parent = _hallwayObject.transform;

                }
            }

            _hallwayObject.transform.parent = _levelObject.transform;
        }

        private void RemoveRoomWalls(Room room)
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
        private void GenerateTileGrid()
        {
            for (int i = 0; i < LevelWidth; i++)
            {
                for (int j = 0; j < LevelLength; j++)
                {
                    Vector3 createAt = new Vector3(i * ObjectSizeOffset, 0, j * ObjectSizeOffset);

                    Tile newTile = new(Tile.TileType.EMPTY, (i, j), createAt);

                    // set the default prefab
                    newTile.TileObject = TilePrefab;

                    tileGrid[i, j] = newTile;
                }
            }
        }
        private void CreateTiles()
        {
            for (int i = 0; i < LevelWidth; i++)
            {
                for (int j = 0; j < LevelLength; j++)
                {
                    GameObject tilePrefab = tileGrid[i, j].TileObject;
                    Vector3 tileLocation = tileGrid[i, j].WorldPosition;

                    // once we've created an instance, we need to reset the reference to that instance rather than the prefab
                    tileGrid[i, j].TileObject = GameObject.Instantiate(tilePrefab, tileLocation, Quaternion.identity);
                }
            }
        }

        private void SimulateRooms()
        {
            int failedPlacements = 0;

            if (FloorTilePrefabs == null || FloorTilePrefabs.Count == 0)
            {
                Debug.LogError("No floor tile prefabs assigned in LevelGenerator.");
                return;
            }

            for (int currentRoom = 0; currentRoom < RoomCount; currentRoom++)
            {
                for (int currentAttempt = 0; currentAttempt < RetryLimit; currentAttempt++)
                {
                    int potentialRoomWidth = UnityEngine.Random.Range(MinRoomWidth, MaxRoomWidth + 1);
                    int potentialRoomLength = UnityEngine.Random.Range(MinRoomLength, MaxRoomLength + 1);
                    int potentialRoomX = UnityEngine.Random.Range(0, LevelWidth - potentialRoomWidth + 1);
                    int potentialRoomY = UnityEngine.Random.Range(0, LevelLength - potentialRoomLength + 1);

                    // pick a random prefab for the room

                    GameObject selectedPrefab = FloorTilePrefabs.ElementAt(UnityEngine.Random.Range(0, FloorTilePrefabs.Count));

                    RectInt potentialRoom = new(potentialRoomX, potentialRoomY, potentialRoomWidth, potentialRoomLength);
                    if (BoundsCheck(potentialRoom))
                    {
                        List<Tile> roomTiles = new();

                        // mark tiles as room tiles
                        foreach (var position in potentialRoom.allPositionsWithin)
                        {
                            var tile = tileGrid[position.x, position.y];
                            // this is important for when we pathfind hallways
                            tile.Type = Tile.TileType.ROOM;

                            tile.TileObject = selectedPrefab;

                            roomTiles.Add(tile);
                        }
                        _placedRooms.Add(new(potentialRoom, null, roomTiles));
                        break;
                    }
                    else if (currentAttempt == RetryLimit - 1)
                    {
                        failedPlacements++;
                    }
                }
            }
        }

        private void RefineRooms()
        {
            int currentRoomIndex = 0;

            foreach (var room in _placedRooms)
            {
                GameObject roomObject = new($"Room {currentRoomIndex + 1}");

                // Collect the tiles/transforms that will belong to this room
                List<Transform> tileTransforms = new();
                List<Renderer> tileRenderers = new();
                List<Collider> tileColliders = new();

                List<Tile> roomTiles = new();

                foreach (var tile in room.RoomTiles)
                {
                    Transform tileTransform = tile.TileObject.transform;
                    tileTransforms.Add(tileTransform);

                    // prefer renderer bounds
                    var tileRenderer = tile.TileObject.GetComponent<Renderer>();
                    if (tileRenderer != null) tileRenderers.Add(tileRenderer);

                    var tileCollider = tile.TileObject.GetComponent<Collider>();
                    if (tileCollider != null) tileColliders.Add(tileCollider);

                    ScaleMaterials(tile.TileObject);
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
                roomObject.transform.position = combinedCenter;
                roomObject.transform.parent = _levelObject.transform;

                room.RoomObject = roomObject;

                // Create a waypoint for the center of each room
                WaypointList.WaypointListRef.Add(new Waypoint() { Position = combinedCenter, Weight = 1 });

                // parent tiles to the room object while preserving their world positions
                foreach (var tile in tileTransforms)
                {
                    tile.SetParent(roomObject.transform, true);
                }

                RemoveRoomWalls(room);

                currentRoomIndex++;
            }
        }

        private void ScaleMaterials(GameObject tile)
        {
            if (tile == null) return;

            float textureScale = ObjectSizeOffset / 10f;
            string uvTilingProperty = "_UV_Tiling";

            MaterialPropertyBlock mpb = new MaterialPropertyBlock();

            // helper to set property if the renderer's material supports it
            void TrySetTiling(Renderer renderer)
            {
                if (renderer == null || renderer.sharedMaterial == null) return;
                if (!renderer.sharedMaterial.HasProperty(uvTilingProperty)) return;

                renderer.GetPropertyBlock(mpb);
                mpb.SetVector(uvTilingProperty, new Vector4(textureScale, textureScale, 0, 0));
                renderer.SetPropertyBlock(mpb);
            }

            // Ceiling
            Transform ceilingTransform = tile.transform.Find("Ceiling");
            if (ceilingTransform != null)
            {
                TrySetTiling(ceilingTransform.GetComponent<Renderer>());
            }

            // Floor
            Transform floorTransform = tile.transform.Find("TileFloor");
            if (floorTransform != null)
            {
                TrySetTiling(floorTransform.GetComponent<Renderer>());
            }

            // Walls
            foreach (var wallEntry in LevelGeneration.Tile.WallNames)
            {
                Transform wallTransform = tile.transform.Find(wallEntry.Value);
                if (wallTransform != null)
                {
                    TrySetTiling(wallTransform.GetComponent<Renderer>());
                }
            }
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

            bool spacedFromOthers = _placedRooms.All(r => !Inflate(r.Bounds, RoomBuffer).Overlaps(room));
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