using System;
using UnityEngine;

namespace Jasper_AI
{
    public class TurnTowards : SteeringBase
    {
        private bool _activeTarget;
        private Vector3 _target;
        [SerializeField] private float turnSpeed;

        private Vector3 _targetDirection;
        private float _angle;
        private float _speed;

        private bool _pathFollow;

        protected override void CalculateMovement()
        {
            if (!_activeTarget) return;
            
            _targetDirection = (_target - transform.position).normalized;
            _angle = Vector3.SignedAngle(transform.forward, _targetDirection, transform.up);

            //if the angle is too small return 
            if (Mathf.Abs(_angle) < 1) return;

            //rotate the appropriate way based on the angle 
            rb.AddTorque(_angle >= 0 ? new Vector3(0, turnSpeed, 0) : new Vector3(0, -turnSpeed, 0));
        }

        public void ChangeTarget(Vector3 newTarget)
        {
            //Debug.Log("Target changed");
            _activeTarget = true;
            _target = newTarget;
        }

        public void ClearTarget()
        {
            _activeTarget = false;
        }

        private void OnDrawGizmos()
        {
            if (!_activeTarget) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, _target);
        }
    }
}
