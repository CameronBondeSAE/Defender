using System.Collections.Generic;
using UnityEngine;
using Anthill.AI;

namespace AshleyPearson
{
    public class Action_MoveToScoutLocation : Anthill.AI.AntAIState
    {
        private GameObject scout;
        [SerializeField] private ScoutLocations scoutLocations;
        private Vector3 targetScoutLocation;
        
        private ScoutMovement scoutMovement;
        
        public override void Create(GameObject gameObject) 
        {   
            //Get reference to scout
            scout = gameObject;
        }

        public override void Enter() //Equivalent to start
        {
            Debug.Log("Entering Action_MoveToScoutLocation");
            
            IdentifyScoutLocation();
            MoveToScoutLocation(targetScoutLocation);
        }

        public void IdentifyScoutLocation()
        {
            //Get a random point to navigate to
            targetScoutLocation = scoutLocations.ChosenScoutLocation();
            if (targetScoutLocation != null) {Debug.Log("[Action_MoveToScoutLocation] Received scout location: " + targetScoutLocation);}
            else {Debug.Log("[Action_MoveToScoutLocation] No scout location received"); }
        }

        public void MoveToScoutLocation(Vector3 targetScoutLocation)
        {
            //If scout is not at scout location, move to scout location
            if (scout.transform.position != targetScoutLocation)
            {
                scoutMovement.MoveTo(targetScoutLocation);
                Debug.Log("[Action_MoveToScoutLocation] Moving to scout location: " + targetScoutLocation);
            }
        }
    }
}
