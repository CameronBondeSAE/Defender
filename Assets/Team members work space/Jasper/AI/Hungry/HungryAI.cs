using Anthill.AI;
using UnityEngine;

namespace Jasper_AI
{
    public class HungryAI : AIBase, ISense
    {
        private enum HungryAIScenario
        {
            SeesFood = 0,
            AtFood = 1,
            EatenFood = 2,
            InFrenzy = 3
        }

        public int biteStrength; 
        
        public bool seesFood, atFood, eatenFood, inFrenzy;

        public GameObject targetFood;
        public bool targetIsAlien; 

        public void CollectConditions(AntAIAgent aAgent, AntAICondition aWorldState)
        {
            aWorldState.Set(HungryAIScenario.SeesFood, seesFood);
            aWorldState.Set(HungryAIScenario.AtFood, atFood);
            aWorldState.Set(HungryAIScenario.EatenFood, eatenFood);
            aWorldState.Set(HungryAIScenario.InFrenzy, inFrenzy);
        }

        protected override void Start()
        {
            base.Start();
            health.OnHealthChanged += HealthChanged;
        }

        private void OnDisable()
        {
            health.OnHealthChanged -= HealthChanged;
        }

        private void HealthChanged(float amount)
        {
            //if has less than 30% of their max health left, put in frenzy 
            if (health.currentHealth.Value < health.maxHealth * .3f) 
            {
                inFrenzy = true; 
            }
            else
            {
                inFrenzy = false;
                //if seeing or at food and tracking a target alien don't track it anymore 
                if (targetIsAlien && (seesFood || atFood))
                {
                    atFood = false; 
                    seesFood = false;
                    targetFood = null;
                    targetIsAlien = false;
                }
            }
        }

        //call when target is destroyed 
        public void TargetDestroyed()
        {
            targetFood = null;
            seesFood = false;
            atFood = false;
        }
    }
}
