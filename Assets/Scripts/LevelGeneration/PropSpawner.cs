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

    public void SpawnNow()
    {
        Transform propsRoot;

        if (levelRoot != null)
        {
            level = levelRoot;
        }
        else
        {
            level = GameObject.Find("Level").transform;
        }

        floors.Clear();
        for (int i = 0; i < level.childCount; i++)
        {
            var child = level.GetChild(i);
            if (child.name.StartsWith(roomNamePrefix))
            {
                CollectFloors(child, floors);
            }
        }

        Transform found = level.Find("Props");
        if (found != null)
        {
            propsRoot = found;
        }
        else
        {
            GameObject newProps = new GameObject("Props");
            propsRoot = newProps.transform;
        }
        
        if (propsRoot.parent == null)
        {
            propsRoot.SetParent(level, false);   
        } 

        for (int i = 0; i < targetCount; i++)
        {
            var p = PickWeighted(props);
            if (p == null || p.Prefab == null) continue;

            var floor = floors[Random.Range(0, floors.Count)];
            var pos   = JitterOnFloor(floor, p.YOffset, p.JitterXZ);
            var yaw   = p.SnapRotation90 ? 90f * Random.Range(0, 4) : Random.Range(0f, 360f);
            var rot   = Quaternion.Euler(0f, yaw, 0f);
            var room  = FindRoomRoot(floor) ?? propsRoot;

            Instantiate(p.Prefab, pos, rot, room).name = p.Prefab.name;
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
            {
                into.Add(current);
            }

            // push all children
            for (int i = 0; i < current.childCount; i++)
            {
                Transform child = current.GetChild(i);
                open.Push(child);
            }
        }
    }


    Transform FindRoomRoot(Transform t)
    {
        while (t != null && t != level)
        {
            if (t.name.StartsWith(roomNamePrefix))
            {
                return t;
            }
            t = t.parent;
        }
        return null;
    }

    Vector3 JitterOnFloor(Transform floor, float yOffset, Vector2 jitter)
    {
        var pos = floor.position + Vector3.up * yOffset;

        var right = floor.right;
        right.y = 0f;
        right.Normalize();

        var fwd = floor.forward;
        fwd.y = 0f;
        fwd.Normalize();

        pos += right * Random.Range(-jitter.x, jitter.x) + fwd * Random.Range(-jitter.y, jitter.y);
        
        return pos;
    }

    SpawnableProp PickWeighted(IList<SpawnableProp> list)
    {
        float total = 0f;

        for (int i = 0; i < list.Count; i++)
        {
            SpawnableProp currentProp = list[i];
            float weight = currentProp.Weight;
            float clampedWeight = Mathf.Max(0f, weight);

            total += clampedWeight;
        }
        
        if (total <= 0f)
        {
            return null;
        }
        float r = Random.Range(0f, total), acc = 0f;
        
        foreach (SpawnableProp p in list)
        {
            float clampedWeight = Mathf.Max(0f, p.Weight);

            acc += clampedWeight;

            if (r <= acc)
            {
                return p;
            }
        }
        return list[list.Count - 1];
    }
}
