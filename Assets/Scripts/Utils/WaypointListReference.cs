using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaypointListReference
{
    public bool UseConstant = true;
    public List<Waypoint> ConstantValue;
    public WaypointListVariable Variable;

    public List<Waypoint> WaypointListRef
    {
        get { return UseConstant ? ConstantValue : Variable.WaypointListVar; }
    }
}
