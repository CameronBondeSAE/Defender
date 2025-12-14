using Anthill.AI;
using UnityEngine;

namespace Jasper_AI
{
    public class GoToFoodState : AntAIState
    {
        private GameObject _parent;
        private HungryAI _sensor;
        
        public override void Create(GameObject go)
        {
            _parent = go; 
            _sensor = go.GetComponent<HungryAI>();
        }

        public override void Enter()
        {
#if UNITY_EDITOR
            Debug.Log($"{_parent.name} is going to food");
#endif
            
            _parent.GetComponent<HungryAI>().MoveTo(_sensor.targetFood.transform.position);
        }
    }
}
