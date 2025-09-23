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

        public GameObject roomObject { get; private set; }

        public Room(RectInt RoomBounds, GameObject roomObject)
        {
            this.bounds = RoomBounds;
            this.roomObject = roomObject;
        }
    }
}
