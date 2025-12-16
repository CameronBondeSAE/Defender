using UnityEngine;

namespace Jasper_AI
{
    public class LookForFoodState : HungryAIBase
    {
        
        public override void Enter()
        {
            sensor.atFood = false;
            sensor.targetFood = null; 
            sensor.inFrenzy = false;
            sensor.seesFood = false;
            sensor.eatenFood = false;
            sensor.MoveSpeed = sensor.DefaultSpeed;
            
            //Debug.Log($"{parent.name} is looking for food");
            pathFollow.StartFollowing(true);
            aboveHeadDisplay.ChangeMessage("Looking for food");
            sensor.health.OnHealthChanged += HealthChanged; 
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
            sensor.health.OnHealthChanged -= HealthChanged;
        }

        private void HealthChanged(float health)
        {
            Debug.Log("Health changed in ai looking for food");
            if (sensor.FrenzyCheck())
            {
                sensor.inFrenzy = true;
                Finish();
            }
        }
    }
}
