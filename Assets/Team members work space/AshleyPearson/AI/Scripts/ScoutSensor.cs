using UnityEngine;
using Anthill.AI;
using Anthill;

namespace AshleyPearson
{
    //The Sensor is the brain. It is in charge of determining when conditions have been met or not met.
    //This script determines what actions the agent will take.
    
    public class ScoutSensor : MonoBehaviour, ISense
    {
        //Local Variables for Planner
        public bool isAlive;
        public bool isNearScoutLocation;
        public bool isNearEnemy;
        public bool isNearPlayer;
        public bool infoToReport;
        public bool hasReportedToPlayer;
        
        //Conditional Enums for Planner
        public enum Scout
        {
            IsNearScoutLocation = 0,
            IsNearPlayer = 1,
            HasReportedToPlayer = 2,
            IsAlive = 3,
            InfoToReport = 4,
            IsNearEnemy = 5
        }

        private void Awake()
        {
            //Set default variable values
            isAlive = true;
            infoToReport = false;
            hasReportedToPlayer = false;
        }

        public void CollectConditions(AntAIAgent aAgent, AntAICondition aWorldState)
        {
            Debug.Log("[ScoutSensor] Collect Conditions is running");
            
            aWorldState.BeginUpdate(aAgent.planner);
            {
                aWorldState.Set(Scout.IsAlive, isAlive);
                aWorldState.Set(Scout.IsNearScoutLocation, isNearScoutLocation);
                aWorldState.Set(Scout.IsNearPlayer, isNearPlayer);
                aWorldState.Set(Scout.IsNearEnemy, isNearEnemy);
                aWorldState.Set(Scout.InfoToReport, infoToReport);
                aWorldState.Set(Scout.HasReportedToPlayer, hasReportedToPlayer);
            }
            
            aWorldState.EndUpdate();
        }

        private void Update()
        {
            if (isAlive)
            {
                //Is the scout close to a scout location?
                
                //Has the scout seen information?
                
                //Is the scout near a player?
                
                //Is the scout near an enemy?
                
                //Has the scout reported to the player?
            }
        }
    }
}
