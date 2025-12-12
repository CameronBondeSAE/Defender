using UnityEngine;
using Anthill.AI;

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

        public int alienCountToReport;
        public bool hasChosenPlayerToReportTo;
        public Transform chosenPlayer;
        
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
        
        //Variables for scout location movement
        private ScoutLocations scoutLocations;

        private void OnEnable()
        {
            ScoutEvents.OnNoInformationFound += ChangeScoutLocation;
            ScoutEvents.OnInformationToReport += InformationToReport;
            ScoutEvents.OnFoundPlayer += ScoutHasChosenPlayerToReportTo;
        }

        private void OnDisable()
        {
            ScoutEvents.OnNoInformationFound -= ChangeScoutLocation;
            ScoutEvents.OnInformationToReport -= InformationToReport;
            ScoutEvents.OnFoundPlayer -= ScoutHasChosenPlayerToReportTo;
        }

        private void Awake()
        {
            //Set default variable values
            isAlive = true;
            infoToReport = false;
            hasReportedToPlayer = false;
            alienCountToReport = 0; //Storing this here so info persists between states
            hasChosenPlayerToReportTo = false; //To be able to check proximity to chosen player
            
            //Check attached scripts
            scoutLocations = GetComponent<ScoutLocations>();
            if (scoutLocations == null) {Debug.Log("[ScoutSensor] Scout Locations script not found");}
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
                CheckProximityToScoutLocation();
                
                //Is the scout close to the chosen player?
                if (hasChosenPlayerToReportTo && !hasReportedToPlayer)
                {
                    CheckProximityToPlayer(chosenPlayer);
                }
                
                //Is the scout near an enemy?

                //Has the scout reported to the player?
            }
        }

        private void CheckProximityToScoutLocation() //this could be abstracted to check for both nearscout and nearplayer
        {
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
            //Pick new scout location 
            scoutLocations.PickRandomNavMeshPointToNavigateTo(); //This isn't protected against re-choosing same location
            isNearScoutLocation = false; //Should push AI back into Move To Scout Location action state
            infoToReport = false; //This should remain false as location is changed BECAUSE no info to report
        }

        private void InformationToReport(int alienCount) //Could maybe add a dataclass here too for other types of info
        {
            //Set alien count to variable
            alienCountToReport = alienCount;
            
            //Set sensor to true to switch into find player/report state
            infoToReport = true;
        }

        private void ScoutHasChosenPlayerToReportTo(Transform player)
        {
            hasChosenPlayerToReportTo = true;
            hasReportedToPlayer = false; //preventative reset
            chosenPlayer = player;
        }
        
        private void CheckProximityToPlayer(Transform player)
        {
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
    }
}
