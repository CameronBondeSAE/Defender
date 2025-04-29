using UnityEngine;

namespace Brad
{
    public class TimeNade : Nades
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public override void Start()
        {
            base.Start();


            if (!hasLaunched) // Only launch if not already launched
            {
                Vector3 launchDirection = transform.forward; // Direction of launch 
                Launch(launchDirection); // Launch function from base class 
            }
        }
        
        protected override void GrenadeLanded()
        {
            base.GrenadeLanded();
            Debug.Log("Slow Grenade triggered.");
        }
    }
}