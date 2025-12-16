using System.Collections.Generic;
using UnityEngine;

namespace Jasper_AI
{
    public class Avoid : SteeringBase
    {
        [SerializeField] private LayerMask avoidMask;
        [SerializeField] private int avoidRange;
        [SerializeField] private float turnStrength;
        [SerializeField] private float slowSpeed;

        private Vector3 _avoidLocation;
        private float _avoidDistance;

        private Vector3 _turnDirection;
        
        private List<GameObject> _exceptions = new List<GameObject>();
        
        public void AddException(GameObject except)
        {
            if (!_exceptions.Contains(except))
            {
                _exceptions.Add(except);
            }
        }

        public void RemoveException(GameObject except)
        {
            if (_exceptions.Contains(except))
            {
                _exceptions.Remove(except);
            }
        }

        protected override void CalculateMovement()
        {
            foreach (RaycastHit hit in look.LookAround(avoidMask))
            {
                if (hit.distance < avoidRange && !_exceptions.Contains(hit.transform.gameObject))
                {
                    Debug.DrawLine(transform.position, hit.point, Color.red);
                    
                    //turn away from object 
                    // turn and slow strength bigger the closer the object is 
                    if (Vector3.SignedAngle(transform.forward, _avoidLocation - transform.position, transform.up) > 0)
                    {
                        rb.AddRelativeTorque(new Vector3(0, -turnStrength * (hit.distance / avoidRange), 0)); //left turn 
                    }
                    else
                    {
                        rb.AddRelativeTorque(new Vector3(0, turnStrength * (hit.distance / avoidRange), 0)); //right turn 
                    }
                }
                
                //back force depending on how close
                //rb.AddForce(Vector3.back * (slowSpeed * (avoidRange / _avoidDistance)));
            }
        }
        
    }
}
