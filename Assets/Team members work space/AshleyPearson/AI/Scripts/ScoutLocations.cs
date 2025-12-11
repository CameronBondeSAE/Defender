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

      private Vector3 chosenScoutLocation;

      private void Awake()
      {
         //Clear existing list
         scoutNavMeshPointsList.Clear();
         
         //Populate initial scout list
         scoutLocationList.Add(scoutLocationOne);
         scoutLocationList.Add(scoutLocationTwo);
         scoutLocationList.Add(scoutLocationThree);
      }
      
      private void Start()
      {
         //Convert list to navmesh points for navigation
         ConvertScoutNavMeshPoints();
         
         //Pick an initial scout point
         PickRandomNavMeshPointToNavigateTo();
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

      private void PickRandomNavMeshPointToNavigateTo()
      {
         if (scoutNavMeshPointsList.Count > 0)
         {
            int i = Random.Range(0, scoutNavMeshPointsList.Count);
            
            chosenScoutLocation = scoutNavMeshPointsList[i];
            Debug.Log("[ScoutLocations] Random scout location picked:  " + chosenScoutLocation);
         }

         else
         {
            Debug.Log("[ScoutLocations] No scout locations found on NavMesh point list");
         }
         
      }

      public Vector3 ChosenScoutLocation()
      {
         return chosenScoutLocation;;
      }
   }
}
