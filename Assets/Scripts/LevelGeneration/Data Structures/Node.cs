using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;
using LevelGeneration;

namespace GraphStructures
{
    // simple class to help with triangles
    // inspiration taken from UnityEngine.ProBuilder and https://github.com/vazgriz/DungeonGenerator
    public class Node : IEquatable<Node>
    {
        public Vector3 Position { get; set; }

        public Room Room { get; set; }

        public Node()
        {

        }

        public Node(Vector3 position)
        {
            Position = position;
            Room = null;
        }

        public Node(Vector3 position, Room room)
        {
            Position = position;
            Room = room;
        }

        public bool Equals(Node other)
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

    public class Node<T> : Node
    {
        public T Item { get; private set; }

        public Node(T item)
        {
            Item = item;
        }

        public Node(Vector3 position, Room room, T item) : base(position, room)
        {
            Item = item;
        }
    }
}