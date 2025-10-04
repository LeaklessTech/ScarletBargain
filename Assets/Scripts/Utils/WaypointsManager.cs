using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class WaypointsManager : MonoBehaviour
{
    public static WaypointsManager Instance { get; private set; }

    public List<Waypoint> waypointList = new List<Waypoint>();

    System.Random rnd = new System.Random();

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        } 
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitWaypoints();
    }

    // Probably won't use this method to generate waypoints, will probably use the LevelGeneration code to do this
    private void GenerateWaypoints()
    {
        throw new NotImplementedException();
    }

    // All waypoints should be spawned as children of the WaypointsManager so as to easily access them (don't want to gather all objects tagged as Waypoint because that is slow)
    private void InitWaypoints()
    {
        foreach (Transform child in transform)
        {
            waypointList.Add(child.gameObject.GetComponent<Waypoint>());
        }
    }


    public Vector3 GetWaypoint()
    {
        int totalWeight = waypointList.Sum(x => x.Weight);

        int randomNumber = rnd.Next(0, totalWeight);

        Waypoint selectedWaypoint = null;
        foreach (Waypoint waypoint in waypointList)
        {
            if (randomNumber < waypoint.Weight)
            {
                selectedWaypoint = waypoint;
                break;
            }

            randomNumber = randomNumber - waypoint.Weight;
        }

        return selectedWaypoint.gameObject.transform.position;
    }
}