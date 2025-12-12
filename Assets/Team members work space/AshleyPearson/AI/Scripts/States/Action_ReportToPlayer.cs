using UnityEngine;
using Anthill.AI;

namespace AshleyPearson
{
    public class Action_ReportToPlayer : AntAIState
    {
        private GameObject scout;
        
        public override void Create(GameObject gameObject) 
        {   
            //Get reference to scout
            scout = gameObject;
        }

        public override void Enter()
        {
            //This state is mostly just to trigger the UI 'report' events and reset the sensor variables
            ScoutEvents.OnReport?.Invoke();
            Debug.Log("[ReportToPlayer] Scout has reported information to player. Event invoked.");
        }
    }
}
