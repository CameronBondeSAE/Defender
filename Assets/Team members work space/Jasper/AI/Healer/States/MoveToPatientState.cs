using UnityEngine;

namespace Jasper_AI
{
    public class MoveToPatientState : HealerAIBase
    {
        private Vector3 _lastTargetPosition;

        public override void Enter()
        {
            //Debug.Log($"{name} moving to patient");
            _lastTargetPosition = sensor.patient.transform.position;
            sensor.MoveTo(_lastTargetPosition);
            aboveHeadDisplay.ChangeMessage("Moving to patient");
            sensor.MoveSpeed = sensor.DefaultSpeed;
        }

        public override void Execute(float aDeltaTime, float aTimeScale)
        {
            //check patient still needs healing 
            if (sensor.patientHealth.currentHealth.Value >= sensor.patientHealth.maxHealth)
            {
                sensor.seesInjured = false;
                Finish();
                return;
            }
            
            //if the patient is within reach
            if (Vector3.Distance(sensor.patient.transform.position, transform.position) < look.Reach)
            {
                sensor.MoveSpeed = 0;
                sensor.atPatient = true;
                Finish();
            }

            //if the patient has moved recalculate path 
            if (_lastTargetPosition != sensor.patient.transform.position)
            {
                _lastTargetPosition = sensor.patient.transform.position;
                
                //if they've moved far enough away for long distance 
                if (Vector3.Distance(_lastTargetPosition, transform.position) > sensor.longDistanceThreshold)
                {
                    sensor.longDistance = true;
                    Finish();
                }
                else //otherwise just move to their new spot
                {
                    sensor.MoveTo(_lastTargetPosition);
                }
            }
        }
    }
}
