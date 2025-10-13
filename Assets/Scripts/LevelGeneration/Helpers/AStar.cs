using GraphStructures;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Utils;

namespace LevelGeneration
{
    public static class AStar
    {
        public enum NeighborDirections { North, South, East, West }

        private static readonly Dictionary<Vector2Int, NeighborDirections> Neighbors = new()
        {
            { new Vector2Int( 0,  1),    NeighborDirections.North },
            { new Vector2Int( 0, -1),    NeighborDirections.South },
            { new Vector2Int( 1,  0),    NeighborDirections.East },
            { new Vector2Int(-1,  0),    NeighborDirections.West }
        };

        public static void PathfindHallways(List<Edge> graph, Tile[,] grid)
        {
            int temp = 0;

            List<Edge> graphEdges = new(graph);

            // pathfind every edge
            foreach (Edge edge in graphEdges)
            {
                // initialize
                Room startingRoom = edge.NodeA.Room;
                Room endingRoom = edge.NodeB.Room;

                Tile startingTile = startingRoom.RoomTiles[UnityEngine.Random.Range(0, startingRoom.RoomTiles.Count)];
                Tile endingTile = endingRoom.RoomTiles[UnityEngine.Random.Range(0, endingRoom.RoomTiles.Count)];

                Tile endOfPath = Pathfind(startingTile, endingTile, grid);

                ResetScores(grid);

                GenerateHallway(endOfPath);

                temp++;
            }

            Debug.Log($"Hallways generated: {temp}");
        }

        // not sure if this is the best way to do it - but we need to reset G scores after creating a hallway
        // otherwise the algorithim will be checking old gScores
        private static void ResetScores(Tile[,] grid)
        {
            foreach(Tile tile in grid)
            {
                tile.ResetScores();
            }
        }

        private static void GenerateHallway(Tile end)
        {
            Tile current = end;

            while (current != null && current.Previous != null)
            {
                current.TileObject.SetActive(true);

                // this shows us the direction of the next tile
                Vector2Int nextTilePosition = current.Previous.Location - current.Location;

                Neighbors.TryGetValue(nextTilePosition, out var neighborDirection);

                switch (neighborDirection)
                {
                    case NeighborDirections.North:
                        current.RemoveWall(Tile.Wall.North);
                        current.Previous.RemoveWall(Tile.Wall.South);
                        break;
                    case NeighborDirections.South:
                        current.RemoveWall(Tile.Wall.South);
                        current.Previous.RemoveWall(Tile.Wall.North);
                        break;
                    case NeighborDirections.East:
                        current.RemoveWall(Tile.Wall.East);
                        current.Previous.RemoveWall(Tile.Wall.West); 
                        break;
                    case NeighborDirections.West:
                        current.RemoveWall(Tile.Wall.West);
                        current.Previous.RemoveWall(Tile.Wall.East);
                        break;
                    default:
                        Debug.LogError("No neighbor found.");
                        break;
                }

                if (current.Type != Tile.TileType.ROOM)
                    current.Type = Tile.TileType.HALLWAY;

                // reset the previous so it can be reused later
                Tile temp = current.Previous;
                current.Previous = null;
                current = temp;
            }
        }

        private static Tile Pathfind(Tile beginning, Tile end, Tile[,] grid)
        {
            PriorityQueue<Tile, int> openSet = new();
            HashSet<Tile> openSetHash = new();

            beginning.gScore = 0;

            if (openSetHash.Add(beginning))
                openSet.Enqueue(beginning, beginning.fScore);

            while (openSet.Count > 0)
            {
                Tile currentTile = openSet.Dequeue();
                openSetHash.Remove(currentTile);

                if (currentTile == end)
                {
                    return currentTile;
                }


                // add valid neighbors
                foreach (var neighbor in GetValidNeighbors(currentTile, grid))
                {
                    int possibleGScore = currentTile.gScore + neighbor.BaseCost;
                    if (possibleGScore < neighbor.gScore)
                    {
                        neighbor.Previous = currentTile;
                        neighbor.gScore = possibleGScore;

                        // replace this later with customizable function
                        neighbor.hScore = ManhattanDistance(neighbor.Location, end.Location);

                        if (!openSetHash.Contains(neighbor))
                        {
                            openSet.Enqueue(neighbor, neighbor.fScore);
                            openSetHash.Add(neighbor);
                        }
                    }
                }
            }

            Debug.Log("Pathfinding failed.");
            return null;
        }

        private static List<Tile> GetValidNeighbors(Tile centerTile, Tile[,] grid)
        {
            List<Tile> validNeighbors = new();

            int matrixWidth = grid.GetLength(0);
            int matrixHeight = grid.GetLength(1);

            foreach (var neighbor in Neighbors)
            {
                Vector2Int potentialLocation = centerTile.Location + neighbor.Key;

                bool locationinsideMatrix =
                 potentialLocation.x >= 0 &&
                 potentialLocation.y >= 0 &&
                 potentialLocation.x < matrixWidth &&
                 potentialLocation.y < matrixHeight;

                if (locationinsideMatrix)
                {
                    Tile neighborTile = grid[potentialLocation.x, potentialLocation.y];
                    validNeighbors.Add(neighborTile);
                }
            }

            return validNeighbors;
        }

        private static int ManhattanDistance(Vector2 positonA, Vector2 positionB)
        {
            return (int)(math.abs(positonA.x - positionB.x) + math.abs(positonA.y - positionB.y));
        }
    }
}
