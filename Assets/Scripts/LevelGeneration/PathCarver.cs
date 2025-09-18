using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LevelGeneration
{
    public partial class LevelGenerator
    {
        // Same Dirs you fixed earlier, aligned with your wall orientation.
        private static readonly (string wall, Vector2Int d, string opposite)[] DirsWithOpp =
        {
        ("NorthWall", new Vector2Int(0,-1), "SouthWall"),
        ("SouthWall", new Vector2Int(0, 1), "NorthWall"),
        ("EastWall",  new Vector2Int(-1,0), "WestWall"),
        ("WestWall",  new Vector2Int( 1,0), "EastWall"),
        };

        private void GenerateHallways()
        {
            // 1) Build room graph and pick connections (MST + optional extra edges)
            var edgesToConnect = BuildConnections(placedRooms, extraEdges: Mathf.CeilToInt(placedRooms.Count * 0.25f));

            // 2) For each connection, pick doorway cells (closest perimeter pair)
            foreach (var (a, b) in edgesToConnect)
            {
                var (fromCell, toCell) = ClosestPerimeterCells(a, b);

                // 3) Carve corridor (simple L-shape for now)
                CarveCorridorL(fromCell, toCell);
            }
        }

        private List<(Room a, Room b)> BuildConnections(List<Room> rooms, int extraEdges = 0)
        {
            var edges = new List<(Room a, Room b, float w)>();
            for (int i = 0; i < rooms.Count; i++)
                for (int j = i + 1; j < rooms.Count; j++)
                    edges.Add((rooms[i], rooms[j], RoomDistance(rooms[i], rooms[j])));

            // Kruskal MST
            var sorted = edges.OrderBy(e => e.w).ToList();
            var parent = new Dictionary<Room, Room>();
            foreach (var r in rooms) parent[r] = r;

            Room Find(Room x) => parent[x] == x ? x : (parent[x] = Find(parent[x]));
            void Union(Room x, Room y) { x = Find(x); y = Find(y); if (x != y) parent[x] = y; }

            var chosen = new List<(Room a, Room b)>();
            foreach (var (a, b, _) in sorted)
            {
                if (Find(a) != Find(b))
                {
                    Union(a, b);
                    chosen.Add((a, b));
                    if (chosen.Count == rooms.Count - 1) break;
                }
            }

            var remaining = sorted
                .Where(e => !chosen.Any(c => (c.a == e.a && c.b == e.b) || (c.a == e.b && c.b == e.a)))
                .Take(extraEdges)
                .Select(e => (e.a, e.b));

            chosen.AddRange(remaining);
            return chosen;
        }

        private static float RoomDistance(Room a, Room b)
        {
            var ac = a.bounds.center; // Vector2int-like center (RectInt gives x+width/2, y+height/2)
            var bc = b.bounds.center;
            return Vector2.Distance((Vector2)ac, (Vector2)bc);
        }

        private static IEnumerable<Vector2Int> PerimeterCells(RectInt r)
        {
            // top/bottom edges
            for (int x = r.xMin; x < r.xMax; x++)
            {
                yield return new Vector2Int(x, r.yMin);
                yield return new Vector2Int(x, r.yMax - 1);
            }
            // left/right edges
            for (int y = r.yMin + 1; y < r.yMax - 1; y++)
            {
                yield return new Vector2Int(r.xMin, y);
                yield return new Vector2Int(r.xMax - 1, y);
            }
        }

        private static (Vector2Int from, Vector2Int to) ClosestPerimeterCells(Room a, Room b)
        {
            var best = (from: new Vector2Int(), to: new Vector2Int(), d: int.MaxValue);

            foreach (var pa in PerimeterCells(a.bounds))
                foreach (var pb in PerimeterCells(b.bounds))
                {
                    int d = Mathf.Abs(pa.x - pb.x) + Mathf.Abs(pa.y - pb.y); 
                    if (d < best.d)
                        best = (pa, pb, d);
                }

            return (best.from, best.to);
        }

        private void CarveCorridorL(Vector2Int start, Vector2Int end)
        {
            bool xFirst = UnityEngine.Random.value < 0.5f;

            Vector2Int cur = start;

            EnsureTile(cur);

            if (xFirst)
            {
                // step x
                int dx = Math.Sign(end.x - cur.x);
                while (cur.x != end.x)
                {
                    var next = new Vector2Int(cur.x + dx, cur.y);
                    CarveStep(cur, next);
                    cur = next;
                }
                // step y
                int dy = Math.Sign(end.y - cur.y);
                while (cur.y != end.y)
                {
                    var next = new Vector2Int(cur.x, cur.y + dy);
                    CarveStep(cur, next);
                    cur = next;
                }
            }
            else
            {
                // step y
                int dy = Math.Sign(end.y - cur.y);
                while (cur.y != end.y)
                {
                    var next = new Vector2Int(cur.x, cur.y + dy);
                    CarveStep(cur, next);
                    cur = next;
                }
                // step x
                int dx = Math.Sign(end.x - cur.x);
                while (cur.x != end.x)
                {
                    var next = new Vector2Int(cur.x + dx, cur.y);
                    CarveStep(cur, next);
                    cur = next;
                }
            }
        }

        private void CarveStep(Vector2Int a, Vector2Int b)
        {
            if (!InBounds(a) || !InBounds(b)) return;

            var tileA = floorGrid[a.x, a.y];
            var tileB = floorGrid[b.x, b.y];
            if (tileA == null || tileB == null) return;

            tileA.SetActive(true);
            tileB.SetActive(true);

            var delta = b - a;

            foreach (var (wall, d, opposite) in DirsWithOpp)
            {
                if (d == delta)
                {
                    RemoveChild(tileA, wall, disable: true);
                    RemoveChild(tileB, opposite, disable: true);
                    break;
                }
            }
        }

        private bool InBounds(Vector2Int p)
        {
            int w = floorGrid.GetLength(0);
            int h = floorGrid.GetLength(1);
            return p.x >= 0 && p.y >= 0 && p.x < w && p.y < h;
        }

        private void EnsureTile(Vector2Int p)
        {
            if (!InBounds(p)) return;
            var t = floorGrid[p.x, p.y];
            if (t != null) t.SetActive(true);
        }
    }

}

