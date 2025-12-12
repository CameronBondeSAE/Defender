
using UnityEngine;
using Anthill.AI;

namespace AshleyPearson
{
    public class Action_MoveToScoutLocation : AntAIState
    {
        private GameObject scout;
        [SerializeField] private ScoutLocations scoutLocations;
        private Vector3 targetScoutLocation;
        
        private ScoutMovement scoutMovement;

        private void OnEnable()
        {
            ScoutEvents.OnScoutReady += EventHandler_MoveToScoutLocation;
        }

        private void OnDisable()
        {
            ScoutEvents.OnScoutReady -= EventHandler_MoveToScoutLocation;
        }

        public override void Create(GameObject gameObject) 
        {   
            //Get reference to scout
            scout = gameObject;
            
            //Get reference to scoutlocations
            scoutLocations = gameObject.GetComponent<ScoutLocations>();
            if(scoutLocations == null) {Debug.Log("[Action_MoveToScoutLocation] No Scout Locations found");}
            
            scoutMovement = gameObject.GetComponent<ScoutMovement>();
            if (scoutMovement == null) {Debug.Log("[Action_MoveToScoutLocation] No Scout Movement found");}
        }

        public override void Enter() //Equivalent to start
        {
            Debug.Log("Entering Action_MoveToScoutLocation");
            
            IdentifyScoutLocation();
            MoveToScoutLocation(targetScoutLocation);
        }
        
        private void IdentifyScoutLocation()
        {
            //Get a random point to navigate to
            scoutLocations.PickRandomNavMeshPointToNavigateTo();
            targetScoutLocation = scoutLocations.ChosenScoutLocation();
            if (targetScoutLocation != null) {Debug.Log("[Action_MoveToScoutLocation] Received scout location: " + targetScoutLocation);}
            else {Debug.Log("[Action_MoveToScoutLocation] No scout location received"); }
        }

        private void EventHandler_MoveToScoutLocation()
        {
            MoveToScoutLocation(targetScoutLocation);
        }

        private void MoveToScoutLocation(Vector3 targetScoutLocation)
        {
            //If scout is not at scout location, move to scout location
            if (scout.transform.position != targetScoutLocation)
            {
                scoutMovement.MoveScout(targetScoutLocation);
                Debug.Log("[Action_MoveToScoutLocation] Moving to scout location: " + targetScoutLocation);
            }
        }
        
        
    }
}
