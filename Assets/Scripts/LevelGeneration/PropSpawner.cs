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
    readonly Dictionary<Transform, List<Transform>> roomToFloors = new(); // Room → List of its floors

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
                var pos = JitterOnFloor(floor, mustProp.YOffset, mustProp.JitterXZ);
                var yaw = mustProp.SnapRotation90 ? 90f * Random.Range(0, 4) : Random.Range(0f, 360f);
                var rot = Quaternion.Euler(0f, yaw, 0f);

                Instantiate(mustProp.Prefab, pos, rot, roomRoot).name = mustProp.Prefab.name;
                spawnedCount++;
            }
        }

        int remaining = targetCount - spawnedCount;
        for (int i = 0; i < remaining; i++)
        {
            var p = PickWeighted(props);
            if (p == null || p.Prefab == null) continue;

            var floor = floors[Random.Range(0, floors.Count)];
            var pos = JitterOnFloor(floor, p.YOffset, p.JitterXZ);
            var yaw = p.SnapRotation90 ? 90f * Random.Range(0, 4) : Random.Range(0f, 360f);
            var rot = Quaternion.Euler(0f, yaw, 0f);
            var room = FindRoomRoot(floor) ?? propsRoot;

            Instantiate(p.Prefab, pos, rot, room).name = p.Prefab.name;
            spawnedCount++;
        }

        Debug.Log($"PropSpawner: Spawned {spawnedCount} props ({mustSpawnProps.Count} types guaranteed per room)");
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
}