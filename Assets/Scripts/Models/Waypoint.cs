using System;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    public int Weight;

    public Vector3 Position { get { return this.gameObject.transform.position; } }
}