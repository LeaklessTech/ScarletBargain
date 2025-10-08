using GraphStructures;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;

public class DelaunayTriangulation
{
    public List<Point> GraphPoints { get; private set; }
    public List<Edge> GraphEdges { get; private set; }
    public List<Triangle> Triangles { get; private set; }


    public DelaunayTriangulation()
    {
        GraphEdges = new List<Edge>();
        Triangles = new List<Triangle>();
    }

    // couple helper functions ispired by Vazgriz, helps with small variation in positioning
    public static bool NearEqual(float a, float b, float tolerance = 0.001f)
    {
        return Mathf.Abs(a - b) <= tolerance;
    }


    public static bool NearEqual(Point left, Point right)
    {
        return NearEqual(left.Position.x, right.Position.x) && NearEqual(left.Position.z, right.Position.z);
    }


    public static DelaunayTriangulation TriangulatePoints(List<Point> points)
    {
        DelaunayTriangulation triangulation = new();
        triangulation.GraphPoints = new(points);

        triangulation.CreateTriangulation();

        return triangulation;
    }

    // Bowyer watson for a 2d triangulation
    private void CreateTriangulation()
    {
        // first, we'll create a "super triangle" that encompasses ALL points

        // we'll need to get the extremes first to make sure we encompass all of them
        float xMin = GraphPoints[0].Position.x;
        float zMin = GraphPoints[0].Position.z;
        float xMax = xMin;
        float zMax = zMin;

        foreach (var point in GraphPoints)
        {
            xMin = Mathf.Min(xMin, point.Position.x);
            zMin = Mathf.Min(zMin, point.Position.z);
            xMax = Mathf.Max(xMax, point.Position.x);
            zMax = Mathf.Max(zMax, point.Position.z);
        }

        // now we know the rough size of how big the triangle needs to be
        float xDiff = xMax - xMin;
        float yDiff = zMax - zMin;
        float dMax = Mathf.Max(xDiff, yDiff) * 2;

        Point A = new Point(new Vector3(xMin - 1, 0, zMin - 1));
        Point B = new Point(new Vector3(xMin - 1, 0, zMax + dMax));
        Point C = new Point(new Vector3(xMax + dMax, 0, zMin - 1));



        // super triangle
        Triangles.Add(new(A, B, C));

        //Debug.DrawLine(A.Position, B.Position, Color.red, 30f);
        //Debug.DrawLine(B.Position, C.Position, Color.red, 30f);
        //Debug.DrawLine(C.Position, A.Position, Color.red, 30f);

        // now we can start inserting points and determining "bad" triangles

        // upon insertion of a new point, if a triangle contains the point in its circumcircle, it is bad
        foreach (var point in GraphPoints)
        {
            List<Edge> poly = new();

            foreach (var triangle in Triangles)
            {
                // create polygon "hole"
                if (triangle.PositionInsideCircumcircle(point.Position))
                {
                    triangle.badTriangle = true;
                    poly.Add(new(triangle.A, triangle.B));
                    poly.Add(new(triangle.B, triangle.C));
                    poly.Add(new(triangle.C, triangle.A));
                }
            }

            //Debug.Log($"Point {point.Position}: bad triangles = {Triangles.Count(t => t.badTriangle)}");

            // get rid of bad triangles
            Triangles.RemoveAll(triangle => triangle.badTriangle);

            for (int i = 0; i < poly.Count; i++)
            {
                for (int j = i + 1; j < poly.Count; j++)
                {
                    if (Edge.NearEqual(poly[i], poly[j]))
                    {
                        poly[i].BadEdge = true;
                        poly[j].BadEdge = true;
                    }
                }
            }

            // foreach doesn't work since we can't modify an IEnumerable while enumerating. makes sense after bashing my head on the wall
            poly.RemoveAll(edge => edge.BadEdge);

            // add good triangles to the list
            foreach (var edge in poly)
            {
                Triangles.Add(new(edge.A, edge.B, point));
            }
        }

        Triangles.RemoveAll(triangle => triangle.ContainsPoint(A.Position) || triangle.ContainsPoint(B.Position) || triangle.ContainsPoint(C.Position));

        // hashset lets us ignore duplicates without issue
        HashSet<Edge> edges = new();

        foreach (var triangle in Triangles)
        {
            Edge edgeA = new(triangle.A, triangle.B);
            Edge edgeB = new(triangle.B, triangle.C);
            Edge edgeC = new(triangle.C, triangle.A);

            if(edges.Add(edgeA))
                GraphEdges.Add(edgeA);

            if (edges.Add(edgeB))
                GraphEdges.Add(edgeB);

            if (edges.Add(edgeC))
                GraphEdges.Add(edgeC);
        }
    }
    
}
