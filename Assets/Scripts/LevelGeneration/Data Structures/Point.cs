using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace GraphStructures
{
    // simple class to help with triangles
    // inspiration taken from UnityEngine.ProBuilder and https://github.com/vazgriz/DungeonGenerator
    public class Point : IEquatable<Point>
    {
        public Vector3 Position { get; set; }

        public Point()
        {

        }

        public Point(Vector3 position) 
        {
            Position = position;
        }

        public bool Equals(Point other)
        {
            if (other == null) 
                return false;

            return DelaunayTriangulation.NearEqual(this, other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class Point<T> : Point
    {
        public T Item { get; private set; }

        public Point(T item)
        {
            Item = item;
        }

        public Point(Vector3 position, T item) : base(position)
        {
            Item = item;
        }
    }
}