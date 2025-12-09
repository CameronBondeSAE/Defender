using Anthill.AI;
using UnityEngine;

namespace Jasper_AI
{
    public class LookForFoodState : AntAIState
    {
        private GameObject _parent;
        private HungryAI _sensor;
        private Look _look;
        
        public override void Create(GameObject go)
        {
            _parent = go; 
            _sensor = go.GetComponent<HungryAI>();
            _look = go.GetComponent<Look>();
        }

        public override void Enter()
        {
            #if UNITY_EDITOR
            Debug.Log($"{_parent.name} is looking for food");
            #endif
        }

        public override void Execute(float aDeltaTime, float aTimeScale)
        {
            foreach (RaycastHit hit in _look.LookAround())
            {
                if (hit.transform.gameObject.TryGetComponent(out UsableItem_Base item))
                {
                    if (item.IsConsumable)
                    {
                        _sensor.targetFood = item.gameObject;
                        _sensor.seesFood = true;
                        Finish();
                    }
                }
            }
        }
    }
}
