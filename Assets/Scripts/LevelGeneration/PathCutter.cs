using GraphStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace LevelGeneration
{
    public partial class LevelGenerator
    {
        public DelaunayTriangulation graph { get; private set; }
        // first we need to generate a Deluaney triangulation
     
        private void CreateGraph()
        {
            // first convert the centers of each room to points
            List<Point> roomCenters = new();

            foreach(var room in placedRooms)
            {
                roomCenters.Add(new Point(room.roomObject.transform.position));
            }

            graph = DelaunayTriangulation.TriangulatePoints(roomCenters);
        }

        private void CreateMST()
        {
            PrimsMST.CalculateMST(graph);
        }
        
        // does some debug.draw to show the graph
        private void DebugGraph()
        {
            foreach(var triangle in graph.Triangles)
            {
                Debug.DrawLine(triangle.A.Position, triangle.B.Position, Color.red, 30f);
                Debug.DrawLine(triangle.B.Position, triangle.C.Position, Color.red, 30f);
                Debug.DrawLine(triangle.C.Position, triangle.A.Position, Color.red, 30f);
            }

        }
    }
}
