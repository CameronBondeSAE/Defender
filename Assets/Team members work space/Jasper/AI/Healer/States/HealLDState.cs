using System.Collections;
using UnityEngine;

namespace Jasper_AI
{
    public class HealLDState : HealerAIBase
    {
        private Health _patientHealth;
        private Vector3 _lastPatientPosition;
        
        public override void Enter()
        {
            //Debug.Log($"{name} healing long distance patient");
            _patientHealth = sensor.patient.GetComponent<Health>();
            _lastPatientPosition = sensor.patient.transform.position;
            turnTowards.ChangeTarget(_lastPatientPosition);
            StartCoroutine(ShootHealing());
            aboveHeadDisplay.ChangeMessage("Healing long distance patient");

            sensor.MoveSpeed = 0;
        }

        public override void Exit()
        {
            turnTowards.ClearTarget();
            StopAllCoroutines();
        }

        private IEnumerator ShootHealing()
        {
            //if the patient has moved
            if (sensor.patient.transform.position != _lastPatientPosition)
            {
                //if closer than the long distance threshold
                if (Vector3.Distance(transform.position, sensor.patient.transform.position) < sensor.longDistanceThreshold)
                {
                    sensor.longDistance = false;
                    Finish();
                    yield break;
                }
                
                _lastPatientPosition = sensor.patient.transform.position;
                turnTowards.ChangeTarget(_lastPatientPosition);
            }
            
            //check patient still needs healing
            if(_patientHealth.currentHealth.Value < _patientHealth.maxHealth)
            {
                sensor.Shoot();
                yield return new WaitForSeconds(sensor.healCooldown);
                StartCoroutine(ShootHealing()); 
            }
            else //otherwise done healing
            {
                sensor.healed = true;
                Finish();
            }
        }
    }
}
