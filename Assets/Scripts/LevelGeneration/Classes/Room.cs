using System.Collections.Generic;
using UnityEngine;

namespace LevelGeneration
{
    /// <summary>
    /// Class to hold definition of a room.
    /// This will be used more in the future for things like destructible walls, items, etc
    /// </summary>
    public class Room
    {
        // note that bounds.x and bounds.y refer to the top left corner
        public RectInt bounds { get; private set; }

        List<GameObject> tiles;

        public Room(RectInt RoomBounds)
        {
            this.bounds = RoomBounds;
        }

        // checks if a given point exists inside the room
        public bool BoundsCheck(Vector2Int point) => bounds.Contains(point);

        public bool Intersects(RectInt other) => bounds.Overlaps(other);


    }
}
