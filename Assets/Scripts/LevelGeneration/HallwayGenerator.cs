using GraphStructures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

namespace LevelGeneration
{
    public partial class LevelGenerator
    {
        public DelaunayTriangulation Graph { get; private set; }
        public MSTResult MST {  get; private set; }
        public HashSet<GraphStructures.Edge> finalGraph { get; private set; }
        // first we need to generate a Deluaney triangulation
        
        private void CreateHallways()
        {
            CreateGraph();
            CreateMST();
            AddRandomEdges();

            AStar.PathfindHallways(new (finalGraph), tileGrid);
        }

        private void CreateGraph()
        {
            // first convert the centers of each room to points
            List<GraphStructures.Node> roomCenters = new();

            foreach(var room in _placedRooms)
            {
                roomCenters.Add(new GraphStructures.Node(room.RoomObject.transform.position, room));
            }

            Graph = DelaunayTriangulation.TriangulatePoints(roomCenters);
        }

        private void CreateMST()
        {
            PrimsMST mst = new(Graph);

            MST = mst.CreateMST();
        }

        private int tempCount;
        private void AddRandomEdges()
        {
            finalGraph = new(MST.TreeEdges);
            HashSet<GraphStructures.Edge> allEdges = new(Graph.GraphEdges);
            allEdges.ExceptWith(finalGraph);

            tempCount = finalGraph.Count;

            foreach(var edge in allEdges)
            {
                float rnd = UnityEngine.Random.Range(0f, 100f);
                if(rnd < AdditionalHallwayChance)
                {
                    finalGraph.Add(edge);
                    Debug.Log($"Edge added: {edge.NodeA.Position},{edge.NodeB.Position}. Rolled {rnd}.");
                }
            }
        }

    }
}
