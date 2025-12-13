using Anthill.AI;
using UnityEngine;

namespace Jasper_AI
{
    public class HealerAI : AIBase, ISense
    {
        [SerializeField] private GameObject healPrefab;
        [SerializeField] private GameObject combatPrefab;
        public int healStrength;
        public float longDistanceThreshold;
        public GameObject patient;
        
        public enum HealerAIScenario
        {
            SeesInjured = 0,
            AbleToHeal = 1,
            HealedInjured = 2,
            InjuredSurrounded = 3,
            InjuredLongDistance = 4
        }
        
        public bool seesInjured, ableToHeal, healedInjured, longDistance, surrounded;

        public void CollectConditions(AntAIAgent aAgent, AntAICondition aWorldState)
        {
            aWorldState.Set(HealerAIScenario.SeesInjured, seesInjured);
            aWorldState.Set(HealerAIScenario.AbleToHeal, ableToHeal);
            aWorldState.Set(HealerAIScenario.HealedInjured, healedInjured);
            aWorldState.Set(HealerAIScenario.InjuredSurrounded, surrounded);
            aWorldState.Set(HealerAIScenario.InjuredLongDistance, longDistance);
        }

        /// <summary>
        /// Shoot a heal or combat projectile in front of 
        /// </summary>
        /// <param name="heal">if the projectile will heal or hurt</param>
        /// <param name="target">target of the projectile</param>
        /// <param name="track">if the projectile should track a target or not</param>
        public void Shoot(bool heal = true, GameObject target = null, bool track = false)
        {
            GameObject shoot = Instantiate(heal ? healPrefab : combatPrefab, (transform.position + transform.forward * 2), transform.rotation);
            
            shoot.GetComponent<Projectile>().SetValues(target, healStrength, track);
        }
    }
}
