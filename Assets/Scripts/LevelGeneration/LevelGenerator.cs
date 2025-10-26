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
        public int RetryLimit = 50;

        [Header("Tile Prefabs")]
        [Tooltip("List of floor tile prefabs to randomly select from for each room")]
        public List<GameObject> FloorTilePrefabs;

        [Tooltip("World-space spacing between tiles (matches tile prefab footprint)")]
        public int ObjectSizeOffset = 10;

        public int Seed;

        public GameObject TilePrefab;

        [Header("Lighting")]

        [Tooltip("List of light prefabs to randomly select from. Each room/hallway uses one prefab.")]

        public List<GameObject> LightPrefabs = new List<GameObject>();

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

            _levelObject = new("Level");

            tileGrid = new Tile[LevelWidth, LevelLength];


            GenerateTileGrid();
            SimulateRooms();
            CreateTiles();
            RefineRooms();
            CreateHallways();
            RemoveRemainingTiles();
            PlaceLights();
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
                    tileGrid[i, j].TileObject = Instantiate(tilePrefab, tileLocation, Quaternion.identity);
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

                // parent tiles to the room object while preserving their world positions
                foreach (var tile in tileTransforms)
                {
                    tile.SetParent(roomObject.transform, true);
                }

                RemoveRoomWalls(room);

                currentRoomIndex++;
            }
        }

        // private bool ScaleMaterials(GameObject Tile)
        // {
        //     float textureScale = ObjectSizeOffset / 10f;
        //     string uvTilingProperty = "_UV_Tiling";

        //     // Scale ceiling material
        //     Transform ceilingTransform = Tile.transform.Find("Ceiling");
        //     if (ceilingTransform != null)
        //         return false;

        //     Renderer ceilingRenderer = ceilingTransform.GetComponent<Renderer>();
        //     if (ceilingRenderer != null)
        //         return false;

        //     Material ceilingMaterial = new Material(ceilingRenderer.sharedMaterial);
        //     ceilingRenderer.material = ceilingMaterial;

        //     if (ceilingMaterial.HasProperty(uvTilingProperty))
        //         ceilingMaterial.SetVector(uvTilingProperty, new Vector4(textureScale, textureScale, 0, 0));


        //     // Scale floor material
        //     Transform floorTransform = Tile.transform.Find("TileFloor");
        //     if (floorTransform != null)
        //         return false;

        //     Renderer floorRenderer = floorTransform.GetComponent<Renderer>();
        //     if (floorRenderer != null)
        //         return false;

        //     Material floorMaterial = new Material(floorRenderer.sharedMaterial);
        //     floorRenderer.material = floorMaterial;

        //     if (floorMaterial.HasProperty(uvTilingProperty))
        //         floorMaterial.SetVector(uvTilingProperty, new Vector4(textureScale, textureScale, 0, 0));


        //     // Scale wall materials
        //     foreach (var Wall in LevelGeneration.Tile.WallNames)
        //     {
        //         Transform wallTransform = Tile.transform.Find(Wall.Value);
        //         if (wallTransform != null)
        //             return false;

        //         Renderer wallRenderer = wallTransform.GetComponent<Renderer>();
        //         if (wallRenderer != null)
        //             return false;

        //         Material wallMaterial = new Material(wallRenderer.sharedMaterial);
        //         wallRenderer.material = wallMaterial;
        //         if (wallMaterial.HasProperty(uvTilingProperty))
        //         {
        //             wallMaterial.SetVector(uvTilingProperty, new Vector4(textureScale, textureScale, 0, 0));
        //         }


        //     }

        //     return true;
        // }

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


        // disabling coroutines for now


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
                    Vector3 createAt = new Vector3(-i * ObjectSizeOffset, 0, -j * ObjectSizeOffset);

                    GameObject newTileGameObject = Instantiate(TilePrefab, createAt, Quaternion.identity);

                    // debug
                    newTileGameObject.transform.GetChild(5).GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{i},{j}";

                    //Tile newTile = new(Tile.TileType.EMPTY, newTileGameObject, (i, j));

                    //tileGrid[i, j] = newTile;

                    // enable the tile so it shows up
                    //newTile.TileObject.SetActive(true);
                }
                yield return new WaitForSeconds(.1f);
            }
        }
        private IEnumerator CreateRoomsCoroutine()
        {
            int failedPlacements = 0;

            for (int currentRoom = 0; currentRoom < RoomCount; currentRoom++)
            {
                for (int currentAttempt = 0; currentAttempt < RetryLimit; currentAttempt++)
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
                        GameObject roomObject = new($"Room {_placedRooms.Count + 1}");
                        roomObject.transform.position = combinedCenter;
                        roomObject.transform.parent = _levelObject.transform;

                        // parent tiles to the room object while preserving their world positions
                        foreach (var t in tileTransforms)
                        {
                            t.SetParent(roomObject.transform, true);
                        }

                        _placedRooms.Add(new(potentialRoom, roomObject, roomTiles));
                        break;
                    }
                    else if (currentAttempt == RetryLimit - 1)
                    {
                        failedPlacements++;
                    }
                }
            }

            foreach (Room room in _placedRooms)
            {
                RemoveRoomWalls(room);
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

        private void PlaceLights()
        {
            if (LightPrefabs == null || LightPrefabs.Count == 0)
            {
                Debug.LogWarning("No light prefabs assigned. Skipping light placement.");
                return;
            }

            // Track all placed light positions globally to enforce no-adjacent rule
            List<Vector2Int> placedLightPositions = new List<Vector2Int>();

            // Place lights for each room (one prefab per room)
            foreach (Room room in _placedRooms)
            {
                if (room.RoomTiles == null || room.RoomTiles.Count == 0) continue;

                // Select one prefab for this entire room
                GameObject roomLightPrefab = LightPrefabs[UnityEngine.Random.Range(0, LightPrefabs.Count)];

                // Get candidate positions (only room tiles)
                List<Vector2Int> candidates = room.RoomTiles
                    .Where(t => t != null)
                    .Select(t => t.Location)
                    .ToList();

                if (candidates.Count == 0) continue;

                // Shuffle for random-ish placement
                var shuffledCandidates = candidates.OrderBy(x => UnityEngine.Random.value).ToList();

                // Greedily place lights, checking global adjacency (Chebyshev distance > 1)
                foreach (Vector2Int pos in shuffledCandidates)
                {
                    bool canPlace = true;
                    foreach (Vector2Int existingPos in placedLightPositions)
                    {
                        int dx = Mathf.Abs(pos.x - existingPos.x);
                        int dy = Mathf.Abs(pos.y - existingPos.y);
                        if (Mathf.Max(dx, dy) <= 1)
                        {
                            canPlace = false;
                            break;
                        }
                    }

                    if (canPlace)
                    {
                        placedLightPositions.Add(pos);

                        // NEW: Calculate ceiling height and spawn just below it
                        Tile tile = tileGrid[pos.x, pos.y];
                        if (tile != null && tile.TileObject != null)
                        {
                            Transform ceilingTransform = tile.TileObject.transform.Find("Ceiling");
                            float ceilingHeight = 5f; // Fallback height (adjust to match your prefabs)
                            if (ceilingTransform != null)
                            {
                                ceilingHeight = ceilingTransform.localPosition.y;
                            }
                            Vector3 spawnPos = tile.WorldPosition + Vector3.up * (ceilingHeight - 0.05f);

                            GameObject lightInstance = Instantiate(roomLightPrefab, spawnPos, Quaternion.identity, tile.TileObject.transform);
                            lightInstance.name = "Light";
                            Debug.Log($"Placed room light at {pos} (ceiling height: {ceilingHeight}) using prefab {roomLightPrefab.name}");
                        }
                    }
                }
            }

            // Place lights for all hallways (one prefab for the entire hallway network)
            List<Tile> hallwayTiles = new List<Tile>();
            for (int x = 0; x < LevelWidth; x++)
            {
                for (int y = 0; y < LevelLength; y++)
                {
                    Tile tile = tileGrid[x, y];
                    if (tile != null && tile.Type == TileType.HALLWAY)
                    {
                        hallwayTiles.Add(tile);
                    }
                }
            }

            if (hallwayTiles.Count > 0)
            {
                // Select one prefab for all hallways
                GameObject hallwayLightPrefab = LightPrefabs[UnityEngine.Random.Range(0, LightPrefabs.Count)];

                // Get candidate positions (only hallway tiles)
                List<Vector2Int> candidates = hallwayTiles
                    .Select(t => t.Location)
                    .ToList();

                // Shuffle for random-ish placement
                var shuffledCandidates = candidates.OrderBy(x => UnityEngine.Random.value).ToList();

                // Greedily place lights, checking global adjacency
                foreach (Vector2Int pos in shuffledCandidates)
                {
                    bool canPlace = true;
                    foreach (Vector2Int existingPos in placedLightPositions)
                    {
                        int dx = Mathf.Abs(pos.x - existingPos.x);
                        int dy = Mathf.Abs(pos.y - existingPos.y);
                        if (Mathf.Max(dx, dy) <= 1)
                        {
                            canPlace = false;
                            break;
                        }
                    }

                    if (canPlace)
                    {
                        placedLightPositions.Add(pos);

                        // NEW: Calculate ceiling height and spawn just below it
                        Tile tile = tileGrid[pos.x, pos.y];
                        if (tile != null && tile.TileObject != null)
                        {
                            Transform ceilingTransform = tile.TileObject.transform.Find("Ceiling");
                            float ceilingHeight = 5f; // Fallback height (adjust to match your prefabs)
                            if (ceilingTransform != null)
                            {
                                ceilingHeight = ceilingTransform.localPosition.y;
                            }
                            Vector3 spawnPos = tile.WorldPosition + Vector3.up * (ceilingHeight - 0.1f);

                            GameObject lightInstance = Instantiate(hallwayLightPrefab, spawnPos, Quaternion.identity, tile.TileObject.transform);
                            lightInstance.name = "Light";
                            Debug.Log($"Placed hallway light at {pos} (ceiling height: {ceilingHeight}) using prefab {hallwayLightPrefab.name}");
                        }
                    }
                }
            }

            Debug.Log($"Light placement complete. Total lights: {placedLightPositions.Count}");
        }

    }
}