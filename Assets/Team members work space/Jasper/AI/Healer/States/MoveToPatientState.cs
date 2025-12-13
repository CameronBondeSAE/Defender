using UnityEngine;

namespace Jasper_AI
{
    public class MoveToPatientState : HealerAIBase
    {
        Vector3 lastTargetPosition;
        
        public override void Execute(float aDeltaTime, float aTimeScale)
        {
            //if the pateint is within reach
            if (Vector3.Distance(sensor.patient.transform.position, transform.position) < look.Reach)
            {
                sensor.ableToHeal = true;
                Finish();
            }

            //if the food has moved recalculate path 
            if (lastTargetPosition != sensor.patient.transform.position)
            {
                sensor.MoveTo(sensor.patient.transform.position);
            }
        }
    }
}
