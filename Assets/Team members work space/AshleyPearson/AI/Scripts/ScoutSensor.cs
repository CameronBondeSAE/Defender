using System.Collections;
using UnityEngine;
using Anthill.AI;
using Unity.Netcode;
using UnityEditor.PackageManager;

namespace AshleyPearson
{
    //The Sensor is the brain. It is in charge of determining when conditions have been met or not met.
    //This script determines what actions the agent will take.
    
    public class ScoutSensor : NetworkBehaviour, ISense
    {
        //Local Variables for Planner
        public bool isAlive;
        public bool isNearScoutLocation;
        public bool isNearEnemy;
        public bool isNearPlayer;
        public bool infoToReport;
        public bool hasReportedToPlayer;
        public bool isAbducted;
        
        //Report variables
        public int alienCountToReport;
        public bool hasChosenPlayerToReportTo;
        public Transform chosenPlayer;
        private bool reportRecencyCoroutineIsRunning = false;
        
        //Enemy variables
        private float enemyDetectionRadius = 3f;
        private LayerMask enemyLayer;
        
        //Conditional Enums for Planner
        public enum Scout
        {
            IsNearScoutLocation = 0,
            IsNearPlayer = 1,
            HasReportedToPlayer = 2,
            IsAlive = 3,
            InfoToReport = 4,
            IsNearEnemy = 5,
            IsAbducted = 6
        }
        
        //Variables for scout location movement
        private ScoutLocations scoutLocations;
        private ScoutMovement scoutMovement;

        private void OnEnable()
        {
            ScoutEvents.OnNoInformationFound += ChangeScoutLocation;
            ScoutEvents.OnInformationToReport += InformationToReport;
            ScoutEvents.OnFoundPlayer += ScoutHasChosenPlayerToReportTo;
            ScoutEvents.OnReport += ScoutHasReported;
            ScoutEvents.OnScoutReady +=  SetScoutVariables;
            
        }

        private void OnDisable()
        {
            ScoutEvents.OnNoInformationFound -= ChangeScoutLocation;
            ScoutEvents.OnInformationToReport -= InformationToReport;
            ScoutEvents.OnFoundPlayer -= ScoutHasChosenPlayerToReportTo;
            ScoutEvents.OnReport -= ScoutHasReported;
            ScoutEvents.OnScoutReady -= SetScoutVariables;
        }

        private void Awake()
        {
            //Check attached scripts
            scoutLocations = GetComponent<ScoutLocations>();
            if (scoutLocations == null) {Debug.Log("[ScoutSensor] Scout Locations script not found");}

            scoutMovement = GetComponent<ScoutMovement>();
            if (scoutLocations == null) {Debug.Log("[ScoutSensor] Scout Movement script not found");}
        }

        private void SetScoutVariables()
        {
            //Set default variable values
            isAlive = true;
            isAbducted = false;
            infoToReport = false;
            hasReportedToPlayer = false;
            alienCountToReport = 0; //Storing this here so info persists between states
            hasChosenPlayerToReportTo = false; //To be able to check proximity to chosen player
        }
        

        public void CollectConditions(AntAIAgent aAgent, AntAICondition aWorldState)
        {
            Debug.Log("[ScoutSensor] Collect Conditions is running");
            
            aWorldState.BeginUpdate(aAgent.planner);
            {
                aWorldState.Set(Scout.IsAlive, isAlive);
                aWorldState.Set(Scout.IsAbducted, scoutMovement.IsAbducted);
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
            //Kick out if not server
            if (!IsServer) return;
            
            //Update whether civ has been abducted or not 
            if (scoutMovement != null)
            {
                isAbducted = scoutMovement.IsAbducted;
                Debug.Log("[ScoutSensor] Scout abduction state:  " + isAbducted);
            }
            
            else {Debug.Log("[ScoutSensor] Scout Movement script not found to update abduction variables");}
            
            //Kick out if scout is dead or abducted
            if (!isAlive || isAbducted) return;
            
            if (isAlive & !isAbducted)
            {
                //Is the scout close to a scout location?
                CheckProximityToScoutLocation();
                
                //Is the scout close to the chosen player?
                if (hasChosenPlayerToReportTo && !hasReportedToPlayer)
                {
                    CheckProximityToPlayer(chosenPlayer);
                }
                
                //Is the scout near an enemy?
                CheckProximityToEnemy();
            }
        }

        private void CheckProximityToScoutLocation() //this could be abstracted to check for both nearscout and nearplayer
        {
            //Kick out if not server
            if (!IsServer) return;
            
            Vector3 targetScoutLocation = scoutLocations.ChosenScoutLocation();

            float distanceTolerance = 2f;
            float distanceToScoutLocation = Vector3.Distance(transform.position , targetScoutLocation);
            //Debug.Log("[ScoutSensor] Scout is: " + distanceToScoutLocation + "away from target scout location" );

            if (distanceToScoutLocation <= distanceTolerance)
            {
                Debug.Log("[ScoutSensor] Scout is near scout location.");
                isNearScoutLocation = true;
            }

            else
            {
                Debug.Log("[ScoutSensor] Scout is NOT near scout location.");
                isNearScoutLocation = false;
            }
        }

        private void ChangeScoutLocation()
        {
            //Kick out if not server
            if (!IsServer) return;
            
            //Pick new scout location 
            scoutLocations.PickRandomNavMeshPointToNavigateTo(); //This isn't protected against re-choosing same location
            isNearScoutLocation = false; //Should push AI back into Move To Scout Location action state
            infoToReport = false; //This should remain false as location is changed BECAUSE no info to report
        }

        private void InformationToReport(int alienCount) //Could maybe add a dataclass here too for other types of info
        {
            //Kick out if not server
            if (!IsServer) return;
            
            //Set alien count to variable
            alienCountToReport = alienCount;
            
            //Set sensor to true to switch into find player/report state
            infoToReport = true;
        }

        private void ScoutHasChosenPlayerToReportTo(Transform player)
        {
            //Kick out if not server
            if (!IsServer) return;
            
            hasChosenPlayerToReportTo = true;
            hasReportedToPlayer = false; //preventative reset
            chosenPlayer = player;
        }
        
        private void CheckProximityToPlayer(Transform player)
        {
            //Kick out if not server
            if (!IsServer) return;
            
            float distanceTolerance = 2f;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= distanceTolerance)
            {
                Debug.Log("[ScoutSensor] Scout is near chosen player.");
                isNearPlayer = true;
            }

            else
            {
                Debug.Log("[ScoutSensor] Scout is NOT near chosen player.");
                isNearPlayer = false;
            }
        }

        private void CheckProximityToEnemy()
        {
            if (!IsServer) return;
            
            Debug.Log("[ScoutSensor] Checking scout proximity to enemy");

            Collider[] hits = Physics.OverlapSphere(transform.position, enemyDetectionRadius, enemyLayer);

            if (hits.Length > 0)
            {
                isNearEnemy = true;
                Debug.Log("[ScoutSensor] Scout is near enemy");
            }

            else
            {
                isNearEnemy = false;
                Debug.Log("[ScoutSensor] Scout is NOT near enemy");
            }

        }

        private void ScoutHasReported(int alienCount)
        {
            //Kick out if not server
            if (!IsServer) return;
            
            if (!reportRecencyCoroutineIsRunning)
            {
                StartCoroutine("ReportRecencyCountdown");
            }

            else
            {
                return;
            }
        }

        private IEnumerator ReportRecencyCountdown()
        {
            reportRecencyCoroutineIsRunning = true;
            
            //Acknowledge the report - don't know if this is even really needed, but just in case
            hasReportedToPlayer = true;
            
            yield return new WaitForSeconds(2f);
            
            ResetAllVariables();

            reportRecencyCoroutineIsRunning = false;
        }

        private void ResetAllVariables()
        {
            //Kick out if not server
            if (!IsServer || isAbducted) return;
            
            //Only not resetting isAlive
            infoToReport = false;
            alienCountToReport = 0;
            
            hasReportedToPlayer = false;
            hasChosenPlayerToReportTo = false;
            chosenPlayer = null;
            
            //Not sure if these will mess things up, but want it to reset to move to scout location
            isNearScoutLocation = false; 
            isNearPlayer = false;
        }
    }
}
