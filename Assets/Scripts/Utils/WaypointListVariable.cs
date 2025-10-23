using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaypointListVariable", menuName = "Scriptable Objects/WaypointListVariable")]
public class WaypointListVariable : ScriptableObject
{
    public List<Waypoint> WaypointListVar;
}