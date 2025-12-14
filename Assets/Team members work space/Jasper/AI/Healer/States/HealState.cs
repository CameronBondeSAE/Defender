using System.Collections;
using UnityEngine;

namespace Jasper_AI
{
    public class HealState : HealerAIBase
    {
        private Health _patientHealth;


        public override void Enter()
        {
            //Debug.Log($"{name} healing patient");
            _patientHealth = sensor.patient.gameObject.GetComponent<Health>();
            StartCoroutine(Heal());
            aboveHeadDisplay.ChangeMessage("Healing patient");
        }

        public override void Exit()
        {
            StopAllCoroutines();
        }

        private IEnumerator Heal()
        {
            //check patient is still within reach 
            if (Vector3.Distance(transform.position, sensor.patient.transform.position) > look.Reach)
            {
                sensor.atPatient = false; 
                Finish();
                yield return null;
            }
            
            _patientHealth.Heal(sensor.healStrength);

            //check the patients health is still below their max health 
            if (_patientHealth.currentHealth.Value < _patientHealth.maxHealth)
            {
                yield return new WaitForSeconds(sensor.healCooldown); //cooldown before healing again 
                StartCoroutine(Heal());
            }
            else //otherwise they are healed
            {
                sensor.healed = true;
                sensor.atPatient = false;
                sensor.patient = null;
                sensor.seesInjured = false;
                Finish();
            }
        }
    }
}
