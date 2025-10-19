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

            foreach(var room in placedRooms)
            {
                roomCenters.Add(new GraphStructures.Node(room.RoomObject.transform.position, room));
            }

            Graph = DelaunayTriangulation.TriangulatePoints(roomCenters);

            foreach(var edge in Graph.GraphEdges)
            {
                Debug.DrawLine(edge.NodeA.Position, edge.NodeB.Position, Color.magenta, 40f);
            }
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

        #region Coroutines

        private IEnumerator HallwayCoroutine()
        {
            CreateGraph();
            CreateMST();

            yield return StartCoroutine(DebugGraphCoroutine());
            yield return StartCoroutine(DebugMSTCoroutine());
            AddRandomEdges();
            yield return StartCoroutine(DebugFinalGraphCoroutine());

            yield return StartCoroutine(AStar.PathfindHallwaysCoroutine(new(finalGraph), tileGrid));
        }

        private IEnumerator DebugGraphCoroutine()
        {
            foreach (var triangle in Graph.Triangles)
            {
                Debug.DrawLine(triangle.A.Position, triangle.B.Position, Color.white, 15f);
                Debug.DrawLine(triangle.B.Position, triangle.C.Position, Color.white, 15f);
                Debug.DrawLine(triangle.C.Position, triangle.A.Position, Color.white, 15f);
                yield return new WaitForSeconds(.1f);
            }

        }
        private IEnumerator DebugMSTCoroutine()
        {
            yield return new WaitForSeconds(5f);
            foreach (GraphStructures.Edge edge in MST.TreeEdges)
            {
                Debug.DrawLine(edge.NodeA.Position + new Vector3(2, 0, 2), edge.NodeB.Position + new Vector3(2, 0, 2), Color.black, 40f);
                yield return new WaitForSeconds(.25f);
            }
        }

        private IEnumerator DebugFinalGraphCoroutine()
        {
            yield return new WaitForSeconds(5f);
            foreach (var edge in finalGraph)
            {
                Debug.DrawLine(edge.NodeA.Position, edge.NodeB.Position, Color.cyan, 120f);
                yield return new WaitForSeconds(.5f);
            }
            Debug.Log($"Added {finalGraph.Count - tempCount} edges. Started with {tempCount}, ended with {finalGraph.Count}. Max was {Graph.GraphEdges.Count}.");
        }

        #endregion
    }
}
