using Anthill.AI;
using UnityEngine;

namespace Jasper_AI
{
    public class AIStateBase : AntAIState//, IAIState
    {
        protected Look look;
        protected TurnTowards turnTowards;
        protected GameObject parent;
        protected Avoid avoid; 
        protected AboveHeadDisplay aboveHeadDisplay;
        protected PathFollow pathFollow;
        
        public override void Create(GameObject go)
        {
            parent = go; 
            look = go.GetComponent<Look>();
            turnTowards = go.GetComponent<TurnTowards>();
            avoid = go.GetComponent<Avoid>();
            aboveHeadDisplay = go.GetComponentInChildren<AboveHeadDisplay>();
            pathFollow = go.GetComponent<PathFollow>();
        }
    }

    public class HungryAIBase : AIStateBase
    {
        protected HungryAI sensor; 
        
        public override void Create(GameObject go)
        {
            base.Create(go); 
            sensor = go.GetComponent<HungryAI>();
        }
    }

    public class HealerAIBase : AIStateBase
    {
        protected HealerAI sensor;

        public override void Create(GameObject go)
        {
            base.Create(go);
            sensor = go.GetComponent<HealerAI>();
        }
    }
}
