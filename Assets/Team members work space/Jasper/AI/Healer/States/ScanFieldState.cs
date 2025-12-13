using UnityEngine;

namespace Jasper_AI
{
    public class ScanFieldState : HealerAIBase
    {
        public override void Enter()
        {
            Debug.Log($"{parent.name} is looking for patients");
        }

        public override void Execute(float aDeltaTime, float aTimeScale)
        {
            //see if any objects in view are usable and consumable 
            foreach (RaycastHit hit in look.LookAround())
            {
                //if an object with an alien tag and health component within sight
                if (hit.collider.gameObject.CompareTag("Alien") && 
                    hit.collider.gameObject.TryGetComponent(out Health health))
                {
                    //if the health is below 70% of its max health 
                    if (health.currentHealth.Value > health.maxHealth * .7f)
                    {
                        break;
                    }
                    
                    sensor.seesInjured = true;
                    
                    //if the hit counts as long distance 
                    if (Vector3.Distance(transform.position, hit.point) > sensor.longDistanceThreshold)
                    {
                        sensor.longDistance = true;
                    }
                    
                    
                    
                }
                
                
                //if sees an alien with less than 80% of health 
                if (hit.transform.gameObject.TryGetComponent(out AlienAI alien) 
                    && alien.health.currentHealth.Value < alien.health.maxHealth * .8f)
                {
                    GameObject alienObject = hit.transform.gameObject;
                    
                    if (Vector3.Distance(parent.transform.position, hit.point) > sensor.longDistanceThreshold)
                    {
                        sensor.longDistance = true;
                        sensor.ableToHeal = true;
                    }
                    
                    sensor.patient = alienObject;
                    Finish();
                }
            }
        }
    }
}
