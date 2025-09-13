using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Level Size")]
    public int LevelWidth = 10;
    public int LevelLength = 10;

    [Header("Rooms")]
    public int MinRoomSize = 2;
    public int MaxRoomSize = 5;
    public int RoomCount = 5;
    [Tooltip("How many failed placement attempts before we give up on adding more rooms")]
    public int retryLimit = 50;

    [Header("Tiles")]
    public GameObject floorTilePrefab;
    [Tooltip("World-space spacing between tiles (matches tile prefab footprint)")]
    public int objectSizeOffset = 10;

    private GameObject[,] floorGrid;
    private bool[,] used; // tiles that belong to rooms

    // For overlap checking
    private readonly List<RectInt> placedRooms = new List<RectInt>();

    void Start()
    {
        BuildBaseGrid();
        PlaceRandomRooms();
        CullUnusedTiles();
        CarveRoomWalls();
    }

    void BuildBaseGrid()
    {
        floorGrid = new GameObject[LevelWidth, LevelLength];
        used = new bool[LevelWidth, LevelLength];

        var parent = new GameObject("FloorGrid").transform;
        parent.SetParent(transform, worldPositionStays: true);

        for (int x = 0; x < LevelWidth; x++)
        {
            for (int z = 0; z < LevelLength; z++)
            {
                Vector3 position = new Vector3(x * objectSizeOffset, 0f, z * objectSizeOffset);
                var tile = Instantiate(floorTilePrefab, position, Quaternion.identity, parent);
                tile.name = $"Tile_{x}_{z}";
                floorGrid[x, z] = tile;
                used[x, z] = false; // default to not used; rooms will mark true
            }
        }
    }

    void PlaceRandomRooms()
    {
        // Clamp sizes
        int minSize = Mathf.Max(1, Mathf.Min(MinRoomSize, MaxRoomSize));
        int maxSize = Mathf.Max(minSize, MaxRoomSize);

        int placed = 0;
        int attempts = 0;

        int padding = 1;

        while (placed < RoomCount && attempts < retryLimit)
        {
            attempts++;

            int rw = Random.Range(minSize, maxSize + 1);
            int rh = Random.Range(minSize, maxSize + 1);

            if (rw > LevelWidth || rh > LevelLength)
                continue;

            // Choose top-left anchor within bounds
            int x = Random.Range(0, LevelWidth - rw + 1);
            int z = Random.Range(0, LevelLength - rh + 1);

            var room = new RectInt(x, z, rw, rh);

            if (OverlapsExisting(room, padding))
                continue;

            // Accept room: mark tiles as used
            for (int ix = room.xMin; ix < room.xMax; ix++)
                for (int iz = room.yMin; iz < room.yMax; iz++)
                    used[ix, iz] = true;

            placedRooms.Add(room);
            placed++;
        }

        if (placed < RoomCount)
        {
            Debug.LogWarning($"LevelGenerator: Only placed {placed}/{RoomCount} rooms after {attempts} attempts. Consider lowering RoomCount, sizes, or padding.");
        }
    }

    bool OverlapsExisting(RectInt room, int padding)
    {
        // Expand by padding and check intersection with any placed room
        var expanded = new RectInt(room.xMin - padding, room.yMin - padding, room.width + 2 * padding, room.height + 2 * padding);
        foreach (var r in placedRooms)
        {
            if (expanded.Overlaps(r))
                return true;
        }
        return false;
    }

    void CullUnusedTiles()
    {
        for (int x = 0; x < LevelWidth; x++)
        {
            for (int z = 0; z < LevelLength; z++)
            {
                if (!used[x, z] && floorGrid[x, z] != null)
                {
                    floorGrid[x, z].SetActive(false); // hide tiles not in any room
                }
            }
        }
    }

    void CarveRoomWalls()
    {
        // For every "used" tile, remove shared walls with used neighbors
        for (int x = 0; x < LevelWidth; x++)
        {
            for (int z = 0; z < LevelLength; z++)
            {
                if (!used[x, z]) continue;

                // North is Z-
                if (IsUsed(x, z - 1))
                {
                    SetWallActive(floorGrid[x, z], "NorthWall", false);
                    SetWallActive(floorGrid[x, z - 1], "SouthWall", false);
                }

                // South is Z+
                if (IsUsed(x, z + 1))
                {
                    SetWallActive(floorGrid[x, z], "SouthWall", false);
                    SetWallActive(floorGrid[x, z + 1], "NorthWall", false);
                }

                // West is X+ 
                if (IsUsed(x + 1, z))
                {
                    SetWallActive(floorGrid[x, z], "WestWall", false);
                    SetWallActive(floorGrid[x + 1, z], "EastWall", false);
                }

                // East is X-
                if (IsUsed(x - 1, z))
                {
                    SetWallActive(floorGrid[x, z], "EastWall", false);
                    SetWallActive(floorGrid[x - 1, z], "WestWall", false);
                }
            }
        }
    }

    bool IsUsed(int x, int z)
    {
        // First check bounds
        bool inBounds = x >= 0 && x < LevelWidth && z >= 0 && z < LevelLength;
        if (!inBounds)
        {
            return false;
        }

        // Grab the tile object and used flag
        GameObject tile = floorGrid[x, z];
        bool markedUsed = used[x, z];

        // Check that it's marked used, exists, and is active
        if (markedUsed && tile != null && tile.activeSelf)
        {
            return true;
        }

        return false;
    }
    void SetWallActive(GameObject tile, string wallName, bool active)
    {
        if (tile == null) return;
        Transform child = tile.transform.Find(wallName);
        if (child != null)
        {
            if (child.gameObject.activeSelf != active)
                child.gameObject.SetActive(active);
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogWarning($"LevelGenerator: Could not find child '{wallName}' on '{tile.name}'. Check prefab child names.");
        }
#endif
    }

}
