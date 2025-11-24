using System.Collections.Generic;
using UnityEngine;

public class PropSpawner : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] Transform levelRoot;
    [SerializeField] string roomNamePrefix = "Room";
    [SerializeField] string floorChildName = "TileFloor";

    [Header("Catalog")]
    [SerializeField] List<SpawnableProp> props = new();

    [Header("Spawn")]
    [SerializeField] int targetCount = 60;

    Transform level;
    readonly List<Transform> floors = new();
    readonly Dictionary<Transform, List<Transform>> roomToFloors = new();

    readonly List<Vector2> placedCentersXZ = new();
    readonly List<float> placedRadii = new();

    readonly Dictionary<SpawnableProp, Vector3> halfCache = new();
    readonly Dictionary<SpawnableProp, float> radiusCache = new();

    const int MaxPlacementAttempts = 12;

    public void SpawnNow()
    {
        if (levelRoot != null) level = levelRoot;
        else level = GameObject.Find("Level")?.transform;

        if (level == null)
        {
            Debug.LogError("PropSpawner: Could not find Level root!");
            return;
        }

        floors.Clear();
        roomToFloors.Clear();

        placedCentersXZ.Clear();
        placedRadii.Clear();

        halfCache.Clear();
        radiusCache.Clear();

        for (int i = 0; i < level.childCount; i++)
        {
            var child = level.GetChild(i);
            if (child.name.StartsWith(roomNamePrefix))
            {
                var roomFloors = new List<Transform>();
                CollectFloors(child, roomFloors);
                if (roomFloors.Count > 0)
                {
                    roomToFloors[child] = roomFloors;
                    floors.AddRange(roomFloors);
                }
            }
        }

        if (floors.Count == 0)
        {
            Debug.LogWarning("PropSpawner: No floors found to spawn props on.");
            return;
        }

        Transform propsRoot = level.Find("Props");
        if (propsRoot == null)
        {
            propsRoot = new GameObject("Props").transform;
            propsRoot.SetParent(level, false);
        }

        int spawnedCount = 0;

        var mustSpawnProps = props.FindAll(p => p.MustSpawnInEveryRoom && p.Prefab != null);

        foreach (var roomPair in roomToFloors)
        {
            Transform roomRoot = roomPair.Key;
            var roomFloors = roomPair.Value;

            foreach (var mustProp in mustSpawnProps)
            {
                var floor = roomFloors[Random.Range(0, roomFloors.Count)];
                if (TrySpawnPropOnFloor(mustProp, floor, roomRoot))
                {
                    spawnedCount++;
                }
                else
                {
                    Debug.LogWarning($"PropSpawner: Failed to place guaranteed prop {mustProp.name} in {roomRoot.name} without overlapping.");
                }
            }
        }

        int remaining = targetCount - spawnedCount;
        for (int i = 0; i < remaining; i++)
        {
            var p = PickWeighted(props);
            if (p == null || p.Prefab == null) continue;

            var floor = floors[Random.Range(0, floors.Count)];
            var room = FindRoomRoot(floor) ?? propsRoot;

            if (TrySpawnPropOnFloor(p, floor, room))
            {
                spawnedCount++;
            }
            else
            {
                Debug.LogWarning($"PropSpawner: Skipped prop {p.name} because no position was found.");
            }
        }
    }

    void CollectFloors(Transform root, List<Transform> into)
    {
        Stack<Transform> open = new Stack<Transform>();
        open.Push(root);

        while (open.Count > 0)
        {
            Transform current = open.Pop();
            if (current.name == floorChildName)
                into.Add(current);

            for (int i = 0; i < current.childCount; i++)
                open.Push(current.GetChild(i));
        }
    }

    Transform FindRoomRoot(Transform t)
    {
        while (t != null && t != level)
        {
            if (t.name.StartsWith(roomNamePrefix))
                return t;
            t = t.parent;
        }
        return null;
    }

    Vector3 JitterOnFloor(Transform floor, float yOffset, Vector2 jitter)
    {
        var pos = floor.position + Vector3.up * yOffset;

        var right = Vector3.ProjectOnPlane(floor.right, Vector3.up).normalized;
        var fwd = Vector3.ProjectOnPlane(floor.forward, Vector3.up).normalized;

        pos += right * Random.Range(-jitter.x, jitter.x);
        pos += fwd * Random.Range(-jitter.y, jitter.y);

        return pos;
    }

    bool TrySpawnPropOnFloor(SpawnableProp prop, Transform floor, Transform parent)
    {
        if (prop?.Prefab == null || floor == null || parent == null)
            return false;

        float radius = GetFootprintRadius(prop);

        for (int attempt = 0; attempt < MaxPlacementAttempts; attempt++)
        {
            var pos = JitterOnFloor(floor, prop.YOffset, prop.JitterXZ);

            if (IsOverlappingXZ(pos, radius))
                continue;

            float yaw = prop.SnapRotation90 ? 90f * Random.Range(0, 4) : Random.Range(0f, 360f);
            var rot = Quaternion.Euler(0f, yaw, 0f);

            Instantiate(prop.Prefab, pos, rot, parent).name = prop.Prefab.name;
            placedCentersXZ.Add(new Vector2(pos.x, pos.z));
            placedRadii.Add(radius);
            
            return true;
        }

        return false;
    }

    bool IsOverlappingXZ(Vector3 position, float radius)
    {
        Vector2 c = new Vector2(position.x, position.z);

        for (int i = 0; i < placedCentersXZ.Count; i++)
        {
            var p = placedCentersXZ[i];
            float radiusOther = placedRadii[i];

            float minDist = radius + radiusOther;
            float minDistSq = minDist * minDist;

            if ((c - p).sqrMagnitude < minDistSq)
                return true;
        }

        return false;
    }
    SpawnableProp PickWeighted(IList<SpawnableProp> list)
    {
        float total = 0f;
        foreach (var p in list)
            total += Mathf.Max(0f, p.Weight);

        if (total <= 0f) return null;

        float r = Random.Range(0f, total);
        float acc = 0f;

        foreach (var p in list)
        {
            float w = Mathf.Max(0f, p.Weight);
            acc += w;
            if (r <= acc)
                return p;
        }
        return list[list.Count - 1];
    }

    Vector3 GetHalfExtents(SpawnableProp prop)
    {
        if (prop == null)
            return new Vector3(0.5f, 0.5f, 0.5f);

        if (halfCache.TryGetValue(prop, out var cached))
            return cached;

        Vector3 half = prop.ManualHalfExtents;

        if (prop.UseRendererBounds && prop.Prefab != null)
        {
            var renderers = prop.Prefab.GetComponentsInChildren<Renderer>(true);
            if (renderers != null && renderers.Length > 0)
            {
                var combined = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    if (renderers[i] != null)
                        combined.Encapsulate(renderers[i].bounds);
                }
                half = combined.extents;
            }
        }

        half = new Vector3(Mathf.Abs(half.x), Mathf.Abs(half.y), Mathf.Abs(half.z));

        float padding = Mathf.Max(0f, prop.BoundsPadding);
        half += new Vector3(padding, padding, padding);

        half = new Vector3(
            Mathf.Max(half.x, 0.01f),
            Mathf.Max(half.y, 0.01f),
            Mathf.Max(half.z, 0.01f)
        );

        halfCache[prop] = half;
        return half;
    }

        float GetFootprintRadius(SpawnableProp prop)
    {
        if (prop == null)
            return 0.5f;

        if (radiusCache.TryGetValue(prop, out float cached))
            return cached;

        Vector3 half = GetHalfExtents(prop);
        // Distance from center to a corner in XZ -> safe for any rotation.
        float radius = new Vector2(half.x, half.z).magnitude;

        radiusCache[prop] = radius;
        return radius;
    }
}
