using UnityEngine;

namespace Jasper_AI
{
    public class FrenzySearchState : HungryAIBase
    {
        public override void Enter()
        {
            sensor.atFood = false;
            sensor.targetFood = null; 
            sensor.seesFood = false;
            sensor.eatenFood = false;
            
            //Debug.Log($"{parent.name} is frenzy eating");
            pathFollow.StartFollowing(true);
            aboveHeadDisplay.ChangeMessage("Frenzied search");
            sensor.health.OnHealthChanged += HealthChanged;
            sensor.MoveSpeed += 2;
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
                    sensor.targetIsLiving = false;
                    item.OnItemDestroyed += sensor.TargetDestroyed;
                    Finish();
                    return;
                }
                
                if (hit.transform.gameObject.TryGetComponent(out Health health) && !health.isDead)
                {
                    sensor.targetFood = health.gameObject;
                    sensor.seesFood = true;
                    sensor.targetIsLiving = true;
                    health.OnDeath += sensor.TargetDestroyed;
                    Finish();
                    return;
                }
            }
        }

        public override void Exit()
        {
            pathFollow.StopFollowing();
            sensor.health.OnHealthChanged -= HealthChanged;
            sensor.MoveSpeed -= 2;
        }
        
        private void HealthChanged(float health)
        {
            if (!sensor.FrenzyCheck())
            {
                sensor.inFrenzy = false;
                Finish();
            }
        }
    }
}
