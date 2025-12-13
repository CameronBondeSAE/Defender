using System.Collections;
using UnityEngine;

namespace Jasper_AI
{
    public class FrenzyEatState : HungryAIBase
    {
        private AlienAI _alien; 
        
        public override void Enter()
        {
#if UNITY_EDITOR
            Debug.Log($"{parent.name} is frenzy eating");
#endif
            
#if UNITY_EDITOR
            Debug.Log($"{parent.name} is eating");
#endif
            if (!sensor.targetIsAlien)
            {
                UsableItem_Base itemBase = sensor.targetFood.GetComponent<UsableItem_Base>();
                UsableItem_Base.ItemRoleForAI role = itemBase.RoleForAI;

                switch (role)
                {
                    //if the food is a snack then eat it and gain health 
                    case UsableItem_Base.ItemRoleForAI.Snack:
                        sensor.health.Heal(1);
                        itemBase.DestroyItem();
                        sensor.eatenFood = true;
                        break;
                    //if the food is a threat then throw it away 
                    case UsableItem_Base.ItemRoleForAI.Threat:
                        itemBase.Launch(transform.forward, 5);
                        sensor.targetFood = null;
                        sensor.seesFood = false;
                        sensor.atFood = false; 
                        break;
                    default:
                        sensor.targetFood = null;
                        sensor.seesFood = false;
                        sensor.atFood = false;
                        break;
                }
                Finish();
            }
            else
            {
                _alien = sensor.targetFood.GetComponent<AlienAI>();
                StartCoroutine(EatAlien());
            }
        }

        /// <summary>
        /// Damages target alien every 2 seconds until they move too far away 
        /// </summary>
        private IEnumerator EatAlien()
        {
            //if alien still close enough then take a bite and wait two seconds again 
            if (Vector3.Distance(transform.position, sensor.targetFood.transform.position) < look.Reach)
            {
                _alien.health.TakeDamage(sensor.biteStrength);

                //if the alien is dead we have eaten 
                if (_alien.health.isDead)
                {
                    sensor.targetFood = null;
                    sensor.eatenFood = true;
                    Finish();
                }
                else
                {
                    yield return new WaitForSeconds(2);
                }
            }
            //otherwise no longer at the food 
            else
            {
                Debug.Log("Alien moved too far away ");
                sensor.atFood = false; 
                Finish();
            }
        }
    }
}
