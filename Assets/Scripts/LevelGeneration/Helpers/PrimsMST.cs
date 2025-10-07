using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using GraphStructures;
using System.Linq;   
using System;
using Utils;
using TMPro;

public class PrimsMST
{

    private List<Point> _graphNodes { get; set; }
    private List<Edge> _graphEdges { get; set; }

    public List<Edge> treeEdges { get; set; } = new();
    public List<Point> treeNodes { get; set; } = new();

    PrimsMST()
    {
        _graphNodes = new();
        treeEdges = new();
        treeNodes = new();
    }

    public static PrimsMST CalculateMST(DelaunayTriangulation triangulation)
    {
        PrimsMST resultTree = new();

        resultTree._graphNodes = triangulation.GraphPoints;
        resultTree._graphEdges = triangulation.GraphEdges;

        resultTree.CreateMST();

        foreach (Edge edge in resultTree.treeEdges)
        {
            Debug.DrawLine(edge.A.Position, edge.B.Position, Color.blue, 30f);
        }

        return null;
    }

    // we use Prims instead, since there is no need to check for loops
    private void CreateMST()
    {
        PriorityQueue<Edge, float> canidateEdges = new();

        // we choose a random point/node as the start
        Point start = _graphNodes[UnityEngine.Random.Range(0, _graphNodes.Count)];

        treeNodes.Add(start);

        // get the initial edges
        foreach (Edge edge in _graphEdges)
        {
            if (edge.A.Equals(start) || edge.B.Equals(start))
                canidateEdges.Enqueue(edge, Vector3.Distance(edge.A.Position, edge.B.Position));
        }

        // keep building the tree 
        while (treeNodes.Count < _graphNodes.Count)
        {
            Edge canidateEdge = canidateEdges.Dequeue();

            // treeNodes having both Points of a canidate edge means this edge would create a loop
            if (treeNodes.Contains(canidateEdge.A) ^ treeNodes.Contains(canidateEdge.B))
            {
                treeEdges.Add(canidateEdge);

                bool newVertexIsA = treeNodes.Contains(canidateEdge.B);
                bool newVertexIsB = treeNodes.Contains(canidateEdge.A);

                if (newVertexIsA)
                {
                    treeNodes.Add(canidateEdge.A);

                    foreach (Edge edge in _graphEdges)
                    {
                        if((edge.A.Equals(canidateEdge.A) || edge.B.Equals(canidateEdge.A)) && !treeEdges.Contains(edge))
                        {
                            canidateEdges.Enqueue(edge, Vector3.Distance(edge.A.Position, edge.B.Position));
                        }
                    }
                }
                else if (newVertexIsB)
                {
                    treeNodes.Add(canidateEdge.B);

                    foreach (Edge edge in _graphEdges)
                    {
                        if ((edge.A.Equals(canidateEdge.B) || edge.B.Equals(canidateEdge.B)) && !treeEdges.Contains(edge))
                        {
                            canidateEdges.Enqueue(edge, Vector3.Distance(edge.A.Position, edge.B.Position));
                        }
                    }
                }
            }
        }
    }
}
