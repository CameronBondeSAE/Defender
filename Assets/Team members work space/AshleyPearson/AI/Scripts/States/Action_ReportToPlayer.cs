using UnityEngine;
using Anthill.AI;
using Unity.Netcode;

namespace AshleyPearson
{
    public class Action_ReportToPlayer : AntAIState
    {
        private GameObject scout;
        private ScoutSensor scoutSensor;
        private int alienCount;
        
        public override void Create(GameObject gameObject) 
        {   
            //Get reference to scout
            scout = gameObject;
            
            //Get reference to scout sensor
            scoutSensor = gameObject.GetComponent<ScoutSensor>();
            if (scoutSensor == null) { Debug.Log("[ReportToPlayer] No scout sensor script found."); }
        }

        public override void Enter()
        {
            //Get alien count from the sensor
            alienCount = scoutSensor.alienCountToReport;
            
            //This state is mostly just to trigger the UI 'report' events and reset the sensor variables
            ScoutEvents.OnReport?.Invoke(alienCount);
            Debug.Log("[ReportToPlayer] Scout has reported information to player. Event invoked.");
        }
    }
}
