using Anthill.AI;
using UnityEngine;

namespace Jasper_AI
{
    public class EatFoodState : HungryAIBase
    {
        private UsableItem_Base _itemBase;
        public override void Enter()
        {
            sensor.MoveSpeed = 0;
            aboveHeadDisplay.ChangeMessage("Eating");
            
            _itemBase = sensor.targetFood.GetComponent<UsableItem_Base>();
            
            UsableItem_Base.ItemRoleForAI role = _itemBase.RoleForAI;

            switch (role)
            {
                //if the food is a snack then eat it and gain health 
                case UsableItem_Base.ItemRoleForAI.Snack:
                    sensor.health.Heal(1);
                    _itemBase.DestroyItem();
                    break;
                default: //anything else throw it
                    _itemBase.Launch(transform.forward, 100);
                    break;
            }

            sensor.eatenFood = true;
            Finish();
        }
    }
}
