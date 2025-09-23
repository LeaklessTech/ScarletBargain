using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LevelGeneration
{
    public partial class LevelGenerator
    {
        // first, we'll need to create a Delaunay Triangulation graph using bowyer-watson

        private List<Vector3> CreateBWDelaunay(GameObject Level)
        {  
            // for debug
            List<Vector3> vector3s = new List<Vector3>();

            Vector3 min = new(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new(float.MinValue, float.MinValue, float.MinValue);

            foreach (var t in Level.GetComponentsInChildren<Transform>())
            {
                if (t == Level.transform) 
                    continue; 
                Vector3 p = t.position; 
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }

            // start by creating super triangle that contains all rooms

            Vector3 center = (min + max) * 0.5f;

            float triangleWidth = max.x - min.x;
            float triangleLength = max.z - min.z;

            float L = Mathf.Max(triangleWidth, triangleLength);
            float triangleScale = Mathf.Max(1f, 1.2f * L); 

            float y0 = center.y;

            Vector3 triangleVertex1 = new(center.x, y0, center.z - 2f * triangleScale);
            Vector3 triangleVertex2 = new(center.x - triangleScale, y0, center.z + triangleScale);
            Vector3 triangleVertex3 = new(center.x + triangleScale, y0, center.z + triangleScale);

            

            return vector3s;
        }

        // once the graph is created, we'll use either Prim's or Kruskal's to create an MST (ensures all rooms are reachable)

        // once MST is created, we'll delete non-MST edges, randomly leaving some (this creates loops)

        // we'll then be left with a (cyclic?) graph with loops - which we'll then use A* to carve paths 
    }
}
