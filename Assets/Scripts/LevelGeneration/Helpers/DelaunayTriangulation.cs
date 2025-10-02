using GraphStructures;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;

public class DelaunayTriangulation
{
    public List<Point> GraphPoints { get; private set; }
    public List<Edge> GraphEdges { get; private set; }
    public List<Triangle> Triangles { get; private set; }


    DelaunayTriangulation()
    {
        GraphEdges = new List<Edge>();
        Triangles = new List<Triangle>();
    }

    // couple helper functions ispired by Vazgriz, helps with small variation in positioning
    public static bool NearEqual(float a, float b)
    {
        return Mathf.Abs(a - b) <= float.Epsilon * Mathf.Abs(a + b) * 2 ||
            Mathf.Abs(a - b) < float.MinValue;
    }

    public static bool NearEqual(Point left, Point right)
    {
        return NearEqual(left.Position.x, right.Position.x) && NearEqual(left.Position.y, right.Position.y);
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
        float yMin = GraphPoints[0].Position.y;
        float xMax = xMin;
        float yMax = yMin;

        foreach (var point in GraphPoints)
        {
            xMin = Mathf.Min(xMin, point.Position.x);
            yMin = Mathf.Min(yMin, point.Position.y);
            xMax = Mathf.Max(xMax, point.Position.x);
            yMax = Mathf.Max(yMax, point.Position.y);
        }

        // now we know the rough size of how big the triangle needs to be
        float xDiff = xMax - xMin;
        float yDiff = yMax - yMin;
        float dMax = Mathf.Max(xDiff, yDiff) * 2;

        Point A = new Point(new(xMin - 1   , yMin - 1      ));
        Point B = new Point(new(xMin - 1   , yMax + dMax   ));
        Point C = new Point(new(xMax + dMax, yMin - 1      ));


        // super triangle
        Triangles.Add(new(A, B, C));

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
                    poly.Add(new(triangle.B,triangle.C));
                    poly.Add(new(triangle.C,triangle.A));
                }
            }

            // get rid of bad triangles
            foreach(var triangle in Triangles)
            {
                if(triangle.badTriangle)
                    Triangles.Remove(triangle);
            }

            for (int i = 0; i < poly.Count; i++)
            {
                for (int j = i + 1; j < poly.Count; j++)
                {
                    if (Edge.AlmostEqual(poly[i], poly[j]))
                    {
                        poly[i].BadEdge = true;
                        poly[j].BadEdge = true;
                    }
                }
            }

            foreach (var edge in poly)
            {
                if(edge.BadEdge)
                    poly.Remove(edge);
            }

            // add good triangles to the list
            foreach (var edge in poly)
            {
                Triangles.Add(new(edge.A, edge.B, point));
            }
        }

        foreach (var triangle in Triangles)
        {
            if (triangle.ContainsPoint(A.Position) || triangle.ContainsPoint(B.Position) || triangle.ContainsPoint(C.Position))
                Triangles.Remove(triangle);
        }

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
