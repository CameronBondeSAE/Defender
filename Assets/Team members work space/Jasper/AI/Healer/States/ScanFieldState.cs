using UnityEngine;

namespace Jasper_AI
{
    public class ScanFieldState : HealerAIBase
    {
        public override void Enter()
        {
            //Debug.Log($"{parent.name} is looking for patients");
            ResetPatrolPoints();
            pathFollow.OnPathEnd += ResetPatrolPoints;
            aboveHeadDisplay.ChangeMessage("Looking for patients");
        }

        public override void Execute(float aDeltaTime, float aTimeScale)
        {
            //see if any objects in view are usable and consumable 
            foreach (RaycastHit hit in look.LookAround())
            {
                //if the hit has an alien tag and health component
                if (!hit.collider.gameObject.CompareTag("Alien") ||
                    !hit.collider.gameObject.TryGetComponent(out Health health))
                {
                    continue;
                }
                //if the health is below 70% of its max health 
                if (health.currentHealth.Value > health.maxHealth * .7f)
                {
                    continue;
                }
                    
                sensor.seesInjured = true;
                    
                //if the hit is further away than the long distance threshold
                if (Vector3.Distance(transform.position, hit.point) > sensor.longDistanceThreshold)
                {
                    sensor.longDistance = true;
                }
                
                sensor.patient = hit.collider.gameObject;
                pathFollow.StopFollowing();
                Finish();
            }
        }

        public override void Exit()
        {
            pathFollow.OnPathEnd -= ResetPatrolPoints;
        }

        private void ResetPatrolPoints()
        {
            sensor.patrolPoints = WaypointManager.Instance.GetUniqueWaypoints(sensor.patrolPointsCount);
            pathFollow.StartFollowing(); 
        }
    }
}
