using System.Collections;
using UnityEngine;

namespace Jasper_AI
{
    public class HealState : HealerAIBase
    {
        private Health patientHealth; 
        
        IEnumerator Heal()
        {
            if (Vector3.Distance(transform.position, sensor.patient.transform.position) > look.Reach)
            {
                sensor.ableToHeal = false; 
                yield return null;
            }
            
            patientHealth.Heal(sensor.healStrength);

            if (patientHealth.currentHealth.Value < patientHealth.maxHealth)
            {
                yield return new WaitForSeconds(2);
            }
            else
            {
                sensor.healedInjured = true;
                Finish();
            }
        }
    }
}
