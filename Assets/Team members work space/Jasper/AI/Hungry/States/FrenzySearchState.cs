using UnityEngine;

namespace Jasper_AI
{
    public class FrenzySearchState : HungryAIBase
    {
        public override void Enter()
        {
#if UNITY_EDITOR
            Debug.Log($"{parent.name} is frenzy eating");
#endif
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
                    sensor.targetIsAlien = false; 
                    Finish();
                }

                if (hit.transform.gameObject.TryGetComponent(out AlienAI alien) && !alien.health.isDead)
                {
                    sensor.targetFood = alien.gameObject;
                    sensor.seesFood = true;
                    sensor.targetIsAlien = true;
                    Finish();
                }
            }
        }
    }
}
