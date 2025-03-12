using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class AlienWaypointSpawner : MonoBehaviour
{
   [FormerlySerializedAs("manualWaypointPrefab")] [SerializeField] private GameObject waypointPrefab;
   [SerializeField] private int waypointCount = 5;
   [SerializeField] private Vector3 spawnAreaSize = new Vector3(10, 0, 10);
   private List<Transform> waypoints = new List<Transform>();

   private void Start()
   {
      for (int i = 0; i < waypointCount; i++)
      {
         Vector3 randomPosition = transform.position + new Vector3
            (Random.Range(-spawnAreaSize.x, spawnAreaSize.x), Random.Range(-spawnAreaSize.z, spawnAreaSize.z));
         GameObject waypoint = Instantiate(waypointPrefab, randomPosition, Quaternion.identity);
         waypoints.Add(waypoint.transform);
      }
   }
   public List<Transform> GetWaypoints() => waypoints;
}
