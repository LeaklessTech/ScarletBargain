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

        public Edge()
        {

        }

        public Edge(Point a, Point b)
        {
            this.A = a;
            this.B = b;
        }

        public bool Equals(Edge edge)
        {
            if((A == edge.A && B == edge.B) || (A == edge.B && B == edge.A))
                return true;
            else
                return false;
        }

        public 
    }
}