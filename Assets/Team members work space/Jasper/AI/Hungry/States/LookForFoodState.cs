using UnityEngine;

namespace Jasper_AI
{
    public class LookForFoodState : HungryAIBase
    {
        
        public override void Enter()
        {
            //Debug.Log($"{parent.name} is looking for food");
            sensor.patrolPoints = WaypointManager.Instance.GetUniqueWaypoints(sensor.patrolPointsCount);
            pathFollow.StartFollowing();
            pathFollow.OnPathEnd += ResetPatrolPoints;
            aboveHeadDisplay.ChangeMessage("Looking for food");
        }

        public override void Execute(float aDeltaTime, float aTimeScale)
        {
            //see if any objects in view are usable and consumable 
            foreach (RaycastHit hit in look.LookAround())
            {
                if (hit.transform.gameObject.TryGetComponent(out UsableItem_Base item) && item.IsConsumable)
                {
                    sensor.targetFood = item.gameObject;
                    sensor.seesFood = true;
                    avoid.AddException(sensor.targetFood);

                    item.OnItemDestroyed += sensor.TargetDestroyed;
                    pathFollow.StopFollowing();
                    Finish();
                }
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
