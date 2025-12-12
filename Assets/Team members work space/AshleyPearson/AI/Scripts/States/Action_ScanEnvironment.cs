using System.Collections.Generic;
using Anthill.AI;
using UnityEngine;

namespace AshleyPearson
{
    public class Action_ScanEnvironment : AntAIState
    {
        private GameObject scout;
        
        //Information for scanner
        private float scanDuration = 3f;
        private float scanTimer;
        private float scanRadius = 12f;

        [SerializeField] private LayerMask alienLayer;
        
        //Information variables
        public int aliensDetected;
        public List<GameObject> aliensDetectedList = new List<GameObject>();
        public bool infoToReport;
        
        public override void Create(GameObject gameObject) 
        {   
            //Get reference to scout
            scout = gameObject;
        }

        public override void Enter() //Equivalent to Start
        {
            Debug.Log("[Action_ScanEnvironment] Entering scan state");
            //Set timer
            scanTimer = scanDuration;
            
            //Set variables
            aliensDetected = 0;
            infoToReport = false;
        }

        public override void Execute(float aDeltaTime, float aTimeScale)
        {
            //Early out if scout already has received info
            if (infoToReport)
            {
                return;
            }
            
            //Countdown timer
            scanTimer-= aDeltaTime;
            
            //These functions could be changed out or prioritised depending on if you wanted the scouts to have additional functionality
            ScanForAliens();
            
            //If nothing is found at current scout location within scan duration, move to new scout point
            if (scanTimer <= 0f)
            {
                if (aliensDetected == 0)
                {
                    //Move on to new scout point 
                    Debug.Log("[Action_ScanEnvironment] No aliens detected after scanning");
                    ScoutEvents.OnNoInformationFound?.Invoke();
                }
            }
        }

        private void ScanForAliens()
        {
            Debug.Log("[Action_ScanEnvironment] Scout is scanning for aliens");
            
            //Clear old list
            aliensDetectedList.Clear();
            
            //Detect aliens
            Collider[] alienHits = Physics.OverlapSphere(scout.transform.position, scanRadius, alienLayer);

            foreach (Collider hit in alienHits)
            {
                aliensDetectedList.Add(hit.gameObject);
            }
            
            aliensDetected = aliensDetectedList.Count;

            if (aliensDetected > 0)
            {
                Debug.Log("[Action_ScanEnvironment] Scout detected aliens to report");
                ScoutEvents.OnInformationToReport?.Invoke(aliensDetected);
            }

            else
            {
                Debug.Log("[Action_ScanEnvironment] Scout detected no aliens detected");
            }
        }

    }
}
