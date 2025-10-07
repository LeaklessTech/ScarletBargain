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
        public Point A { get; set; }
        public Point B { get; set; }
        public bool BadEdge { get; set; }

        public Edge()
        {

        }

        public Edge(Point a, Point b)
        {
            this.A = a;
            this.B = b;
        }

        // just some helpful overrides to make code cleaner
        public static bool operator ==(Edge edgeA, Edge edgeB)
        {
            return (edgeA.A == edgeB.A || edgeA.A == edgeB.B)
                && (edgeA.B == edgeB.A || edgeA.B == edgeB.B);
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
            return HashCode.Combine(A, B);
        }

        // alternate version of equals that will give us a little more leeway 
        public static bool NearEqual(Edge left, Edge right)
        {
            return DelaunayTriangulation.NearEqual(left.A, right.A) && DelaunayTriangulation.NearEqual(left.B, right.B)
                || DelaunayTriangulation.NearEqual(left.A, right.B) && DelaunayTriangulation.NearEqual(left.B, right.A);
        }
    }
}