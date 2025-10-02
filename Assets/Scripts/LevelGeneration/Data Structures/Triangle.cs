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
    ///  </summary>
    public bool PositionInsideCircumcircle(Vector3 position)
    {
        // get our 3 triangle corners
        Vector3 a = A.Position;
        Vector3 b = B.Position;
        Vector3 c = C.Position;

        // get the squared lengths
        float aSqr = a.sqrMagnitude;
        float bSqr = b.sqrMagnitude;
        float cSqr = c.sqrMagnitude;

        // compute the circumcenter coordinates
        float circumcenterX = (aSqr * (c.y - b.y) + bSqr * (a.y - c.y) + cSqr * (b.y - a.y)) / (a.x * (c.y - b.y) + b.x * (a.y - c.y) + c.x * (b.y - a.y));

        float circumcenterY = (aSqr * (c.x - b.x) + bSqr * (a.x - c.x) + cSqr * (b.x - a.x)) / (a.y * (c.x - b.x) + b.y * (a.x - c.x) + c.y * (b.x - a.x));

        // now we can calculate the circumcenter point
        Vector3 circum = new(circumcenterX / 2, circumcenterY / 2);

        // and the radius (squared)
        float circumcircleRadius = Vector3.SqrMagnitude(a - circum);

        // now we can figure out the distance from the position to the center, allowing us to detect if its inside the circumcircle
        float distance = Vector3.SqrMagnitude(position - circum);

        return distance <= circumcircleRadius;
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
