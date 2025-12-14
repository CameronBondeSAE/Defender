using System.Collections.Generic;
using UnityEngine;
using Anthill.AI;

namespace AshleyPearson
{
    public class Action_Run : AntAIState
    {
        private GameObject scout;
        private ScoutMovement scoutMovement;
        private RunLocations runLocations;

        private List<Transform> runLocationsList = new List<Transform>();
        private Vector3 targetRunLocation;
        
        public override void Create(GameObject gameObject) 
        {   
            //Get reference to scout
            scout = gameObject;
            
            //Get reference to scoutlocations
            runLocations = gameObject.GetComponent<RunLocations>();
            if(runLocations == null) {Debug.Log("[Action_Run] No Run Locations script found");}
            
            scoutMovement = gameObject.GetComponent<ScoutMovement>();
            if (scoutMovement == null) {Debug.Log("[Action_Run] No Scout Movement found");}
        }
        
        public override void Enter() //Equivalent to start
        {
            Debug.Log("Entering Action_Run for scout");
            
            IdentifyRunLocation();
            MoveToRunLocation(targetRunLocation);
        }
        
        private void IdentifyRunLocation()
        {
            //Get a random point to navigate to
            runLocations.PickRandomNavMeshPointToRunTo();
            targetRunLocation = runLocations.ChosenRunLocation();
            if (targetRunLocation != null) {Debug.Log("[Action_Run] Received scout run location: " + targetRunLocation);}
            else {Debug.Log("[Action_Run] No scout run location received"); }
        }
        
        //Shouldn't need an event handler since it's highly unlikely the scout will be spawned next to aliens ever
        
        private void MoveToRunLocation(Vector3 targetRunLocation)
        {
            //If scout is not at scout location, move to scout location
            if (scout.transform.position != targetRunLocation)
            {
                scoutMovement.MoveScout(targetRunLocation);
                Debug.Log("[Action_Run] Moving to run location: " + targetRunLocation);
            }
        }

    }
}
