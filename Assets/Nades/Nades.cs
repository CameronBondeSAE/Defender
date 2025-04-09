using UnityEngine;


namespace Brad
{
    public class Nades : MonoBehaviour
    {
        public GrenadeData grenadeData;
        protected float countdown;
        protected float launchSpeed;
        protected Rigidbody rb;
        private Transform startPosition;
        [SerializeField] LineRenderer grenadeTrajectory;

        public virtual void Start()
        {
            
        }

        public virtual void Update()
        {
           
        }

        public void Launch()
        {
            Vector3 launchDirection = transform.forward + transform.up * 1f;
            rb.AddForce(launchDirection * launchSpeed, ForceMode.VelocityChange);
        }

    }
}