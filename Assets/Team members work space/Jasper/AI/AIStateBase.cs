using Anthill.AI;
using UnityEngine;

namespace Jasper_AI
{
    public class AIStateBase : AntAIState//, IAIState
    {
        public Look look;
        public GameObject parent;
        
        public override void Create(GameObject go)
        {
            parent = go; 
            look = go.GetComponent<Look>();
        }

        public void Stay()
        {
            
        }
    }

    public class HungryAIBase : AIStateBase
    {
        public HungryAI sensor; 
        
        public override void Create(GameObject go)
        {
            base.Create(go); 
            sensor = go.GetComponent<HungryAI>();
        }
    }

    public class HealerAIBase : AIStateBase
    {
        public HealerAI sensor;

        public override void Create(GameObject go)
        {
            base.Create(go);
            sensor = go.GetComponent<HealerAI>();
        }
    }
}
