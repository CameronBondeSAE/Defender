using UnityEngine;

namespace AshleyPearson
{
    public class Action_FindPlayer : Anthill.AI.AntAIState
    {
        private GameObject scout;
        private Transform targetPlayer;
        
        [SerializeField] private GameObject[] playerArray = new GameObject[4];
        private ScoutMovement scoutMovement;
        
        //Timer variables
        private float checkInterval = 2f;
        private float checkTimer;
        
        public override void Create(GameObject gameObject)
        {
            //Get reference to scout
            scout = gameObject;
            
            //Get other references
            scoutMovement = scout.GetComponent<ScoutMovement>();
            if (scoutMovement == null) { Debug.Log("[FindPlayer] No scout movement script found."); }
        }

        public override void Enter()
        {
            Debug.Log("[FindPlayer] Scout entering find player state");
            
            if (playerArray.Length == 0)
            {
                Debug.LogWarning("[FindPlayer] No player for scout to report to found");
            }
            
            //Figure out which player is the closest player
            targetPlayer = GetClosestPlayer();

            if (targetPlayer != null)
            {
                Debug.Log("[FindPlayer] Scout found player: " + targetPlayer.name);
                ScoutEvents.OnFoundPlayer?.Invoke(targetPlayer);
            }

            else
            {
                Debug.Log("[FindPlayer] No player for scout to report to found");
            }
            
            checkTimer = checkInterval;
        }

        public override void Execute(float aDeltaTime, float aTimeScale)
        {
            if (targetPlayer == null) return;
            
            //Occasionally recheck if there is a new closest player 
            if (playerArray.Length > 1) //Only search if more than one player
            {
                RecheckClosestPlayer(aDeltaTime);
            }
            
            //Target's position needs to be continuously updated so scout can follow as player moves around
            scoutMovement.MoveScout(targetPlayer.position);
        }

        private void RecheckClosestPlayer(float aDeltaTime)
        {
            //Decrease timer
            checkTimer -= aDeltaTime;

            if (checkTimer <= 0)
            {
                //Re-get closest player
                targetPlayer = GetClosestPlayer();
                
                //Reset timer
                checkTimer = checkInterval;

                if (targetPlayer != null)
                {
                    Debug.Log("[FindPlayer] Scout recalculated closest player. Now reporting to " + targetPlayer.name);
                }
                
            }
        }

        private Transform GetClosestPlayer()
        {
            //Populate player list with players currently in the game
            playerArray = GameObject.FindGameObjectsWithTag("Player");
            
            //Set values to be overridden
            Transform closestPlayer = null;
            float closestDistance = Mathf.Infinity;
            Vector3 scoutPosition = scout.transform.position;

            foreach (GameObject player in playerArray)
            {
                float distance = Vector3.Distance(scoutPosition, player.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player.transform;
                }
            }
            
            return closestPlayer;
        }
    }
}