using System.Collections.Generic;
using UnityEngine;

namespace LevelGeneration
{
    public class Tile
    {
        const int HALLWAY_COST = 1;
        const int ROOM_COST = 5;
        const int EMPTY_COST = 15;

        public enum TileType { HALLWAY, ROOM, EMPTY }
        public enum Wall { North, South, East, West}

        private static readonly Dictionary<Wall, string> WallNames = new()
        {
            { Wall.North, "NorthWall"},
            { Wall.South, "SouthWall"},
            { Wall.East, "EastWall" },
            { Wall.West, "WestWall" }
        };

        private TileType _type;

        public TileType Type
        {
            get => _type;
            set
            {
                _type = value;
                BaseCost = value switch
                {
                    TileType.HALLWAY => HALLWAY_COST,
                    TileType.ROOM => ROOM_COST,
                    TileType.EMPTY => EMPTY_COST,
                    _ => BaseCost
                };
            }
        }

        public int BaseCost { get; set; }
        public GameObject TileObject { get; set; }

        public Vector2Int Location;

        public Tile Previous { get; set; }

        // we want fScore to be updated based on the changes in H and G score, so we use some backing fields
        private int _gScore;
        private int _hScore;

        public int gScore
        {
            get => _gScore;
            set => _gScore = value;
        }

        public int hScore
        {
            get => _hScore;
            set => _hScore = value;
        }

        public int fScore => _gScore + _hScore;

        public Tile(TileType type, GameObject tileObject, (int x, int y) location)
        {
            this.Type = type;

            this.TileObject = tileObject;

            Location = new(location.x, location.y);

            gScore = int.MaxValue;
        }

        public bool RemoveWall(Wall wallToRemove)
        {
            if (!WallNames.TryGetValue(wallToRemove, out string wallName))
                return false;

            Transform wallTransform = TileObject.transform.Find(wallName);

            if (wallTransform == null) 
                return false;

            GameObject.Destroy(wallTransform.gameObject);

            return true;
        }

        public void ResetScores()
        {
            _gScore = int.MaxValue;
            _hScore = 0;
        }
    }
}
