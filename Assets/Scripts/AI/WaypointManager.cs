using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance;

    public int totalWaypoints = 24;
    public float waypointSpacing = 10f;
    public float checkRadius = 1f;
    public LayerMask blockLayer;
    public Vector3 areaCenter;
    public Vector3 areaSize;
    
    private List<Transform> allWaypoints = new List<Transform>();
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        GenerateWaypoints();
    }

    void GenerateWaypoints()
    {
        int attempts = 0;

        while (allWaypoints.Count < totalWaypoints && attempts < totalWaypoints * 10)
        {
            // Generate a random point within the area bounds
            Vector3 randomOffset = new Vector3(
                Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
                0,
                Random.Range(-areaSize.z / 2f, areaSize.z / 2f)
            );

            Vector3 randomPos = areaCenter + randomOffset;

            // Check for valid NavMesh position near random point
            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                Vector3 validPos = hit.position;

                // Check there's no object in the blocking layer at this point
                if (!Physics.CheckSphere(validPos, checkRadius, blockLayer))
                {
                    // Extra check to make sure this isn't too close to any existing waypoint
                    bool tooClose = false;
                    foreach (var waypoint in allWaypoints)
                    {
                        if (Vector3.Distance(waypoint.position, validPos) < waypointSpacing)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (!tooClose)
                    {
                        // Create a new waypoint
                        GameObject waypoint = new GameObject("Waypoint_" + allWaypoints.Count);
                        waypoint.transform.position = validPos;

                        allWaypoints.Add(waypoint.transform);

                        Debug.DrawRay(validPos + Vector3.up * 1f, Vector3.up * 0.5f, Color.green, 5f); // visible ray
                    }
                }
            }

            attempts++;
            Debug.Log($"Generated {allWaypoints.Count} unique waypoints.");
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        foreach (var wp in allWaypoints)
        {
            if (wp != null)
                Gizmos.DrawSphere(wp.position, 0.3f);
        }
    }
    

    // Returns a list of unique waypoints for a single AI
    public Transform[] GetUniqueWaypoints(int count)
    {
        List<Transform> result = new List<Transform>();
        HashSet<int> used = new HashSet<int>(); // Make sure no duplicates

        while (result.Count < count && used.Count < allWaypoints.Count)
        {
            int index = Random.Range(0, allWaypoints.Count);

            // If not already chosen, add it
            if (!used.Contains(index))
            {
                used.Add(index);
                result.Add(allWaypoints[index]);
            }
        }

        return result.ToArray();
    }
}
