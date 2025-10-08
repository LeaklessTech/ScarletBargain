using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using GraphStructures;
using System.Linq;   
using System;
using Utils;
using TMPro;

public class MSTResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public HashSet<Edge> TreeEdges { get; set; }
    public HashSet<Point> TreeNodes { get; set; }

    public MSTResult()
    {
        TreeEdges = new();
        TreeNodes = new();
    }
}

public class PrimsMST
{
    private List<Point> _graphNodes { get; set; }
    private List<Edge> _graphEdges { get; set; }

    public PrimsMST(DelaunayTriangulation triangulation)
    {
        _graphNodes = triangulation.GraphPoints;
        _graphEdges = triangulation.GraphEdges;
    }

    // we use Prims instead, since there is no need to check for loops
    public MSTResult CreateMST()
    {
        if (_graphNodes == null || _graphEdges == null)
            return new MSTResult { Success = false, Message = "Graph nodes/edges not properly initialized" };

        if(_graphNodes.Count < 2)
            return new MSTResult { Success = false, Message = "At least 2 graph nodes required." };

        if (_graphEdges.Count < 1)
            return new MSTResult { Success = false, Message = "At least 1 graph edge required." };

        MSTResult result = new();

        PriorityQueue<Edge, float> canidateEdges = new();

        // we choose a random point/node as the start
        Point start = _graphNodes[UnityEngine.Random.Range(0, _graphNodes.Count)];

        result.TreeNodes.Add(start);

        // get the initial edges
        foreach (Edge edge in _graphEdges)
        {
            if (edge.A.Equals(start) || edge.B.Equals(start))
                canidateEdges.Enqueue(edge, edge.weight);
        }

        // keep building the tree 
        while (result.TreeNodes.Count < _graphNodes.Count)
        {
            if (canidateEdges.Count == 0)
                return new MSTResult { Success = false, Message = "Empty canidateEdge queue, potentially disconnected graph." };

            Edge canidateEdge = canidateEdges.Dequeue();

            // treeNodes having both Points of a canidate edge means this edge would create a loop
            if (result.TreeNodes.Contains(canidateEdge.A) ^ result.TreeNodes.Contains(canidateEdge.B))
            {
                result.TreeEdges.Add(canidateEdge);   

                bool newVertexIsA = result.TreeNodes.Contains(canidateEdge.B);
                bool newVertexIsB = result.TreeNodes.Contains(canidateEdge.A);

                if (newVertexIsA)
                {
                    result.TreeNodes.Add(canidateEdge.A);

                    foreach (Edge edge in _graphEdges)
                    {
                        if((edge.A.Equals(canidateEdge.A) || edge.B.Equals(canidateEdge.A)) && !result.TreeEdges.Contains(edge))
                        {
                            canidateEdges.Enqueue(edge, edge.weight);
                        }
                    }
                }
                else if (newVertexIsB)
                {
                    result.TreeNodes.Add(canidateEdge.B);

                    foreach (Edge edge in _graphEdges)
                    {
                        if ((edge.A.Equals(canidateEdge.B) || edge.B.Equals(canidateEdge.B)) && !result.TreeEdges.Contains(edge))
                        {
                            canidateEdges.Enqueue(edge, edge.weight);
                        }
                    }
                }
            }
        }

        result.Success = true;
        return result;
    }
}
