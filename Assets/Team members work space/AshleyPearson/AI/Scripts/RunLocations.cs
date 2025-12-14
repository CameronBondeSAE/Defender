using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace AshleyPearson
{
    //To determine a run location.
    //In hindsight, much of this could have been made into shared base class for run/scout locations inheriting from it
    
    public class RunLocations : NetworkBehaviour
    {
        [SerializeField] private Transform runLocationOne;
        [SerializeField] private Transform runLocationTwo;
        [SerializeField] private Transform runLocationThree;
        
        [SerializeField] private List<Transform> runLocationList = new List<Transform>();
        [SerializeField] private List<Vector3> runNavMeshPointsList = new List<Vector3>() ;

        private Vector3 chosenRunLocation;

        private void OnEnable()
        {
           ScoutEvents.OnScoutReady += SetRunLocations;
        }

        private void OnDisable()
        {
           ScoutEvents.OnScoutReady -= SetRunLocations;
        }
       
       private void SetRunLocations()
         {
            //Kick out if not server
            if (!IsServer) return;
            
            //Clear existing list
            runNavMeshPointsList.Clear();
            
            //Populate initial scout list
            runLocationList.Add(runLocationOne);
            runLocationList.Add(runLocationTwo);
            runLocationList.Add(runLocationThree);
            
            //Convert list to navmesh points for navigation
            ConvertRunNavMeshPoints();
         }

         private void ConvertRunNavMeshPoints()
         {
            //Kick out if not server
            if (!IsServer) return;
            
            foreach (Transform runLocation in runLocationList)
            {
               //Add point to list if position is walkable
               if (NavMesh.SamplePosition(runLocation.position, out NavMeshHit navMeshHit, 5f, NavMesh.AllAreas))
               {
                  Debug.Log("[RunLocations] Run navmesh point has been added");
                  runNavMeshPointsList.Add(navMeshHit.position);
               }

               else
               {
                  Debug.LogWarning("[RunLocations] " + runLocation.name + "is not walkable on navmesh");
               }
            }
            Debug.Log("[RunLocations] There are: " + runNavMeshPointsList.Count + "run locations"); //Should be 3 currently
         }

         public void PickRandomNavMeshPointToRunTo()
         {
            //Kick out if not server
            if (!IsServer) return;
            
            if (runNavMeshPointsList.Count > 0)
            {
               int i = Random.Range(0, runNavMeshPointsList.Count);
               
               chosenRunLocation = runNavMeshPointsList[i];
               Debug.Log("[RUnLocations] Random run location picked:  " + chosenRunLocation);
            }

            else
            {
               Debug.Log("[RunLocations] No run locations found on NavMesh point list");
            }
            
         }

         public Vector3 ChosenRunLocation()
         {
            return chosenRunLocation;;
         }
    }
}
