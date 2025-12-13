using Anthill.AI;
using UnityEngine;

namespace Jasper_AI
{
    public class EatFoodState : HungryAIBase
    {
        private UsableItem_Base _itemBase;
        public override void Enter()
        {

            Debug.Log($"{parent.name} is eating");
            
            _itemBase = sensor.targetFood.GetComponent<UsableItem_Base>();
            
            UsableItem_Base.ItemRoleForAI role = _itemBase.RoleForAI;

            switch (role)
            {
                //if the food is a snack then eat it and gain health 
                case UsableItem_Base.ItemRoleForAI.Snack:
                    sensor.health.Heal(1);
                    _itemBase.DestroyItem();
                    sensor.eatenFood = true;
                    break;
                case UsableItem_Base.ItemRoleForAI.Threat:
                    _itemBase.Launch(transform.forward, 115);
                    break;
            }
            
            sensor.targetFood = null;
            sensor.seesFood = false;
            sensor.atFood = false; 
            Finish();
        }
    }
}
