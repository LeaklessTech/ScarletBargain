using GraphStructures;
using System;
using UnityEngine;

// inspired by https://github.com/vazgriz/DungeonGenerator
// utilizes a couple helper structures to create triangle
// we'll use this to create the delaunay triangulation graph
public class Triangle : IEquatable<Triangle>
{
    public bool badTriangle { get; set; }

    public Point A { get; private set; }
    public Point B { get; private set; }
    public Point C { get; private set; }

    public Triangle()
    {

    }

    public Triangle(Point a, Point b, Point c)
    {
        this.A = a;
        this.B = b;
        this.C = c;
    }

    /// <summary>
    ///  A circumcircle is the circle created by the 3 points of a triangle in 2D space.
    ///  Here, we calculate if a position is inside this triangle's circumcircle.
    ///  We later use this to figure out if a triangle is "bad" or not.
    ///  
    /// 
    ///  </summary>
    public bool PositionInsideCircumcircle(Vector3 p)
    {
        float ax = A.Position.x;
        float az = A.Position.z;
        float bx = B.Position.x;
        float bz = B.Position.z;
        float cx = C.Position.x;
        float cz = C.Position.z;
        float px = p.x;
        float pz = p.z;

        float ab = (ax * ax + az * az - bx * bx - bz * bz) / 2;
        float ac = (ax * ax + az * az - cx * cx - cz * cz) / 2;

        float det = (ax - bx) * (az - cz) - (ax - cx) * (az - bz);

        // since we're working with a dungeon that generates from a grid, we'll often end up with colinear (lined up) rooms
        // this causes issues for a native version of Bowyer-Watson
        // returning false for colinear rooms fixes this
        if (Mathf.Abs(det) < 1e-6f) return false; // nearly colinear

        float cx_circum = (ab * (az - cz) - ac * (az - bz)) / det;
        float cz_circum = ((ax - bx) * ac - (ax - cx) * ab) / det;

        float radiusSqr = (ax - cx_circum) * (ax - cx_circum) + (az - cz_circum) * (az - cz_circum);
        float distSqr = (px - cx_circum) * (px - cx_circum) + (pz - cz_circum) * (pz - cz_circum);

        return distSqr <= radiusSqr + 1e-6f;
    }




    // simple helper method
    // we give some tolerance for small variation in position
    public bool ContainsPoint(Vector3 point)
    {
        return Vector3.Distance(point, A.Position) < 0.01f ||
               Vector3.Distance(point, B.Position) < 0.01f ||
               Vector3.Distance(point, C.Position) < 0.01f;
    }

    // a couple cool operator overrides - not completely necessary, but helps make future code much cleaner
    public static bool operator ==(Triangle triangleA, Triangle triangleB)
    {
        return (triangleA.A == triangleB.A || triangleA.A == triangleB.B || triangleA.A == triangleB.C) &&
               (triangleA.B == triangleB.A || triangleA.B == triangleB.B || triangleA.B == triangleB.C) &&
               (triangleA.C == triangleB.A || triangleA.C == triangleB.B || triangleA.C == triangleB.C);
    }

    public static bool operator !=(Triangle triangleA, Triangle triangleB)
    {
        return !(triangleA == triangleB);
    }

    public bool Equals(Triangle triangle)
    {
        return this == triangle;
    }

    public override bool Equals(object obj)
    {
        if(obj is Triangle triangle)
        {
            return this == triangle;
        }
        else
            return false;
    }

    // we use XOR here as a computationally ch
    public override int GetHashCode()
    {
        return HashCode.Combine(A, B, C);
    }
}
