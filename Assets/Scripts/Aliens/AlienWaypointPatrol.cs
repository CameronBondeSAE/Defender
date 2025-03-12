using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AlienWaypointPatrol : AlienMovement
{
    [SerializeField] private List<Transform> waypoints;
    private int currentWaypointIndex = 0;
    public AlienWaypointSpawner waypointSpawner;

    protected override void Awake()
    {
        base.Awake();
        if (waypoints.Count > 0)
            target = waypoints[currentWaypointIndex].position;
        waypoints = waypointSpawner.GetWaypoints();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate(); 
    }

    protected override void OnTargetReached()
    {
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        target = waypoints[currentWaypointIndex].position;
    }
}
