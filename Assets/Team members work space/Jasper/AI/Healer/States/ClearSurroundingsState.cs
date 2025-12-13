using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jasper_AI
{
    public class ClearSurroundingsState : HealerAIBase
    {
        private List<Collider> surroundingAI;

        private GameObject target; 
        private Health targetHealth;

        public override void Enter()
        {
            //get all the ais surrounding the current thing we are trying to heal 
            foreach (Collider c in look.CheckSurroundingArea(sensor.patient.transform.position, 3))
            {
                if (c.TryGetComponent(out AIBase ai))
                {
                    surroundingAI.Add(c);
                }
            }
            
            NewTarget();
        }

        public override void Exit()
        {
            StopAllCoroutines();
        }

        IEnumerator ShootTarget()
        {
            if (targetHealth.isDead)
            {
                NewTarget();
            }
            else
            {
                sensor.Shoot(false); 
                yield return new WaitForSeconds(3); 
            }
        }

        private void NewTarget()
        {
            if (surroundingAI.Count < 3)
            {
                sensor.surrounded = false;
                Finish();
            }
            
            Collider newTarget = surroundingAI[Random.Range(0, surroundingAI.Count)];
            targetHealth = newTarget.GetComponent<Health>();
            //to do make it turn towards the target 

            StartCoroutine(ShootTarget());
        }
    }
}
