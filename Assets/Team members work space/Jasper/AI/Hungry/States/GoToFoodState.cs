using System;
using Anthill.AI;
using UnityEngine;

namespace Jasper_AI
{
    public class GoToFoodState : HungryAIBase
    {
        private Vector3 lastTargetPosition;
        private bool _changedSpeed; 
        public override void Enter()
        {
            //Debug.Log($"{parent.name} is going to food");
            aboveHeadDisplay.ChangeMessage("Going to food");

            //make faster if in a frenzy
            if (sensor.inFrenzy)
            {
                Debug.Log("Frenzy speed");
                sensor.MoveSpeed += 2; 
                _changedSpeed = true;
            }
            
            lastTargetPosition = sensor.targetFood.transform.position;
            sensor.MoveTo(lastTargetPosition);
        }

        public override void Execute(float aDeltaTime, float aTimeScale)
        {
            //make sure food still exists 
            if (sensor.targetFood is null)
            {
                Finish();
                return;
            }
            
            //if the food has moved recalculate path 
            if (lastTargetPosition != sensor.targetFood.transform.position)
            {
                lastTargetPosition = sensor.targetFood.transform.position;
                sensor.MoveTo(lastTargetPosition);
            }
            //if the food is close enough to reach
            else if (Vector3.Distance(lastTargetPosition, transform.position) < look.Reach)
            {
                Debug.Log("Reached food.");
                sensor.atFood = true;
                Finish();
            }
        }
        
        public override void Exit()
        {
            if (_changedSpeed)
            {
                sensor.MoveSpeed -= 2;
            }
        }

        private void OnDrawGizmos()
        {
            if (sensor.targetFood is null) return; 
            
            //pink line to target food
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, sensor.targetFood.transform.position);
        }
    }
}
