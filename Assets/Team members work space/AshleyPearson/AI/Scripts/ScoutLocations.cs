using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace AshleyPearson
{

   public class ScoutLocations : MonoBehaviour
   {
      [SerializeField] private Transform scoutLocationOne;
      [SerializeField] private Transform scoutLocationTwo;
      [SerializeField] private Transform scoutLocationThree;

      [SerializeField] List<Transform> scoutLocationList = new List<Transform>();
      [SerializeField] List<Vector3> scoutNavMeshPointsList = new List<Vector3>();

      //Convert to navmesh points for navigation
      private void Start()
      {
         //Clear existing list
         scoutNavMeshPointsList.Clear();
      }

      private void ConvertScoutNavMeshPoints()
      {
         foreach (Transform scoutLocation in scoutLocationList)
         {
            //Add point to list if position is walkable
            if (NavMesh.SamplePosition(scoutLocation.position, out NavMeshHit navMeshHit, 5f, NavMesh.AllAreas))
            {
               Debug.Log("[ScoutLocations] Scout navmesh point has been added");
               scoutNavMeshPointsList.Add(navMeshHit.position);
            }

            else
            {
               Debug.LogWarning("[ScoutLocations] " + scoutLocation.name + "is not walkable on navmesh");
            }
         }
         
         Debug.Log("[ScoutLocations] There are: " + scoutNavMeshPointsList.Count + "scout locations"); //Should be 3 currently
      }

      private Vector3 PickRandomNavMeshPointToNavigateTo()
      {
         if (scoutNavMeshPointsList.Count > 0)
         {
            int i = Random.Range(0, scoutNavMeshPointsList.Count);
            Vector3 randomNavMeshPoint = scoutNavMeshPointsList[i];
            
            Debug.Log("[ScoutLocations] Random scout location picked:  " + randomNavMeshPoint);
            return randomNavMeshPoint;
         }

         else
         {
            Debug.Log("[ScoutLocations] No scout locations found on NavMesh point list");
            return Vector3.zero;
         }
      }

      public List<Vector3> ScoutNavMeshList()
      {
         return scoutNavMeshPointsList;
      }

      public Vector3 GetRandomScoutNavMeshPoint()
      {
         return PickRandomNavMeshPointToNavigateTo();
      }
   }
}
