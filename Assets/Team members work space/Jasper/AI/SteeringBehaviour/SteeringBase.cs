using UnityEngine;

namespace Jasper_AI
{
    public class SteeringBase : MonoBehaviour
    {
        protected Rigidbody rb;
        protected Look look;

        protected void Start()
        {
            rb = GetComponent<Rigidbody>();
            look = GetComponent<Look>();
        }

        protected void FixedUpdate()
        {
            CalculateMovement();
        }

        protected virtual void CalculateMovement()
        {
            
        }
}
}
