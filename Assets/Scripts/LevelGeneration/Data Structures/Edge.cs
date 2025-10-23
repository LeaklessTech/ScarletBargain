using System;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;

namespace GraphStructures
{
    // another simple class to assist in creating the triangulation
    // lets us connect two points
    // inspired by https://github.com/vazgriz/DungeonGenerator
    public class Edge : IEquatable<Edge>
    {
        public Node NodeA { get; set; }
        public Node NodeB { get; set; }
        public bool BadEdge { get; set; }

        public float weight { get; set; }

        public Edge()
        {

        }

        public Edge(Node a, Node b)
        {
            this.NodeA = a;
            this.NodeB = b;

            weight = Vector3.Distance(NodeA.Position, NodeB.Position);
        }

        // just some helpful overrides to make code cleaner
        public static bool operator ==(Edge edgeA, Edge edgeB)
        {
            return (edgeA.NodeA == edgeB.NodeA || edgeA.NodeA == edgeB.NodeB)
                && (edgeA.NodeB == edgeB.NodeA || edgeA.NodeB == edgeB.NodeB);
        }

        public static bool operator !=(Edge edgeA, Edge edgeB)
        {
            return !(edgeA == edgeB);
        }

        public bool Equals(Edge edge)
        {
            return this == edge;
        }

        public override bool Equals(object obj)
        {
            if (obj is Edge edge)
            {
                return this == edge;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(NodeA, NodeB);
        }

        // alternate version of equals that will give us a little more leeway 
        public static bool NearEqual(Edge left, Edge right)
        {
            return DelaunayTriangulation.NearEqual(left.NodeA, right.NodeA) && DelaunayTriangulation.NearEqual(left.NodeB, right.NodeB)
                || DelaunayTriangulation.NearEqual(left.NodeA, right.NodeB) && DelaunayTriangulation.NearEqual(left.NodeB, right.NodeA);
        }
    }
}