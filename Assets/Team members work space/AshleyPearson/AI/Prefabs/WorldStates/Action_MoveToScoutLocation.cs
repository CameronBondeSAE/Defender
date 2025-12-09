using System.Collections.Generic;
using UnityEngine;
using Anthill.AI;

namespace AshleyPearson
{
    public class Action_MoveToScoutLocation : Anthill.AI.AntAIState
    {
        private GameObject scout;
        private ScoutLocations scoutLocations;
        private Vector3 currentScoutLocation;

        public override void Create(GameObject gameObject) 
        {   
            //Get reference to scout
            scout = gameObject;
        }

        public override void Enter() //Equivalent to start
        {
            Debug.Log("Entering Action_MoveToScoutLocation");
            
            //Get a random point to navigate to
            currentScoutLocation = scoutLocations.GetRandomScoutNavMeshPoint();
            if (currentScoutLocation != null) {Debug.Log("[Action_MoveToScoutLocation] Received scout location: " + currentScoutLocation);}
        }
    }
}
