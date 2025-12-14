using Anthill.AI;
using UnityEngine;

namespace Jasper_AI
{
    public class HealerAI : AIBase, ISense
    {
        [SerializeField] private GameObject healPrefab;
        public LayerMask healLayer;
        public int healStrength;
        public float longDistanceThreshold;
        public GameObject patient;
        public float healCooldown;
        public Health patientHealth;
        
        public enum HealerAIScenario
        {
            SeesPatient = 0,
            Healed = 1,
            PatientLongDistance = 2,
            AtPatient = 3
        }

        public bool seesInjured, atPatient, healed, longDistance;

        public void CollectConditions(AntAIAgent aAgent, AntAICondition aWorldState)
        {
            aWorldState.Set(HealerAIScenario.SeesPatient, seesInjured);
            aWorldState.Set(HealerAIScenario.AtPatient, atPatient);
            aWorldState.Set(HealerAIScenario.Healed, healed);
            aWorldState.Set(HealerAIScenario.PatientLongDistance, longDistance);
        }

        /// <summary>
        /// Shoots a healing projectile just in front of the AI 
        /// </summary>
        /// <param name="target">target of the projectile</param>
        /// <param name="track">if the projectile should track a target or not</param>
        public void Shoot(GameObject target = null, bool track = false)
        {
            Debug.Log($"{name} shooting");
            GameObject shoot = Instantiate(healPrefab, (transform.position + transform.forward * 2), transform.rotation);
            
            shoot.GetComponent<Projectile>().SetValues(target, healStrength, track);
        }
    }
}
