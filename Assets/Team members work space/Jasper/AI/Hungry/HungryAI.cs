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
        public bool targetIsLiving; 

        public void CollectConditions(AntAIAgent aAgent, AntAICondition aWorldState)
        {
            aWorldState.Set(HungryAIScenario.SeesFood, seesFood);
            aWorldState.Set(HungryAIScenario.AtFood, atFood);
            aWorldState.Set(HungryAIScenario.EatenFood, eatenFood);
            aWorldState.Set(HungryAIScenario.InFrenzy, inFrenzy);
        }

        public bool FrenzyCheck()
        {
            //if has less than 30% of their max health left, put in frenzy 
            if (health.currentHealth.Value < health.maxHealth * .3f) 
            {
                return true; 
            }
            return false;
            
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
