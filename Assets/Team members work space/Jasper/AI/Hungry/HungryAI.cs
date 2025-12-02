using Anthill.AI;
using UnityEngine;

namespace Jasper_AI
{
    public class HungryAI : AlienAI, ISense
    {
        public enum HungryAIScenario
        {
            SeesFood = 0,
            AtFood = 1,
            EatenFood = 2
        }

        public bool seesFood, atFood, eatenFood; 

        public void CollectConditions(AntAIAgent aAgent, AntAICondition aWorldState)
        {
            aWorldState.Set(HungryAIScenario.SeesFood, seesFood);
            aWorldState.Set(HungryAIScenario.AtFood, eatenFood);
            aWorldState.Set(HungryAIScenario.EatenFood, atFood);
        }
    }
}
