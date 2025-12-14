using UnityEngine;

namespace Jasper_AI
{
    public class ScanFieldState : HealerAIBase
    {
        public override void Enter()
        {
            sensor.seesInjured = false;
            sensor.patient = null;
            sensor.atPatient = false;
            sensor.healed = false; 
            
            //Debug.Log($"{parent.name} is looking for patients");
            aboveHeadDisplay.ChangeMessage("Looking for patients");
            pathFollow.StartFollowing(true);

            sensor.MoveSpeed = sensor.DefaultSpeed; 
        }

        public override void Execute(float aDeltaTime, float aTimeScale)
        {
            
            //see if any objects in view are usable and consumable 
            foreach (RaycastHit hit in look.LookAround(sensor.healLayer))
            {
                //if the hit doesnt have a health component
                if (!hit.collider.gameObject.TryGetComponent(out Health health))
                {
                    continue;
                }
                //if the health is above 70% of its max health 
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
                sensor.patientHealth = health;
                Finish();
            }
        }

        public override void Exit()
        {
            pathFollow.StopFollowing();
        }
    }
}
