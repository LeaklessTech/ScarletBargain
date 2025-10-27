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
        public RectInt Bounds { get; private set; }
        public GameObject RoomObject { get; set; }
        public List<Tile> RoomTiles { get; private set; }
        public int FloorPrefabIndex { get; private set; }

        public Room(RectInt RoomBounds, GameObject roomObject, List<Tile> roomTiles, int floorPrefabIndex)
        {
            this.Bounds = RoomBounds;
            this.RoomObject = roomObject;
            RoomTiles = roomTiles;
            FloorPrefabIndex = floorPrefabIndex;
        }
    }
}
