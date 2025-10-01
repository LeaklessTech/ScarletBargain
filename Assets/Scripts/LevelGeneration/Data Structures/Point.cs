using System;
using UnityEngine;

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

        public Point(Vector3 location) 
        {
            Position = location;
        }

        public bool Equals(Point vertex)
        {
            return vertex.Position == this.Position;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class Point<T> : Point
    {
        public T Item { get; set; }

        public Point(T item)
        {
            Item = item;
        }

        public Point(Vector3 location, T item) : base(location)
        {
            Item = item;
        }
    }
}