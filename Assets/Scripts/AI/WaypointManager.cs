using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance;

    private List<Transform> allWaypoints = new List<Transform>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        FindWaypointsInScene();
    }

    // Finds all objects in the scene with the "waypoint" tag
    private void FindWaypointsInScene()
    {
        allWaypoints.Clear();
        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag("waypoint");

        foreach (GameObject obj in waypointObjects)
        {
            allWaypoints.Add(obj.transform);
        }
        
        Debug.Log($"Found {allWaypoints.Count} waypoints in the scene.");
    }

    // void OnDrawGizmos() // circle out the waypoints to see
    // {
    //     Gizmos.color = Color.blue;
    //     foreach (var wp in allWaypoints)
    //     {
    //         if (wp != null)
    //             Gizmos.DrawSphere(wp.position, 0.3f);
    //     }
    // }

    // Returns a list of unique waypoints for a single AI
    public Transform[] GetUniqueWaypoints(int count)
    {
        List<Transform> result = new List<Transform>();
        HashSet<int> used = new HashSet<int>();

        while (result.Count < count && used.Count < allWaypoints.Count)
        {
            int index = Random.Range(0, allWaypoints.Count);

            if (!used.Contains(index))
            {
                used.Add(index);
                result.Add(allWaypoints[index]);
            }
        }

        return result.ToArray();
    }

    public Transform[] GetAllWaypoints() // backup function to add all waypoints in the scene to an AI.
    {
        return allWaypoints.ToArray();
    }
}
