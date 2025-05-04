using System.Collections;
using UnityEngine;

namespace Brad
{
    public class TimeNade : Nades
    {
        private Coroutine MoveAndDelete;  // Moves the time grenade activating on trigger exit then deletes 

        public override void Start()
        {
            base.Start();

            if (!hasLaunched)
            {
                Vector3 launchDirection = transform.forward;
                Launch(launchDirection);
            }
        }

        /// <summary>
        /// Called when the grenade lands
        /// </summary>
        protected override void GrenadeLanded()
        {
            Debug.Log("Slow Grenade triggered.");

          
            transform.localScale *= 11f;  
            rb.useGravity = false;
            rb.isKinematic = true;

            
            Collider collider = GetComponentInChildren<Collider>(); // Makes the collider a trigger 
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            if (MoveAndDelete != null)
            {
                StopCoroutine(MoveAndDelete); // Stop any existing Coroutine 
            }

            MoveAndDelete = StartCoroutine(DeleteTimeGrenade());
        }

        /// <summary>
        /// Steps to delete the time grenade
        /// </summary>
        /// <returns></returns>
        private IEnumerator DeleteTimeGrenade()
        {
            yield return new WaitForSeconds(3.0f); // Grenade duration? 
            transform.localPosition = new Vector3(0.0f, -80f, 0f); // Drops then grenade to trigger OnTriggerExit
            
            yield return new WaitForSeconds(3.0f); // Delay just to make sure it worked
            

            Debug.Log("Delete time grenade.");
            Destroy(this.gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {    
            Debug.Log("Entered trigger: " + other.name);
            Rigidbody rb = other.GetComponentInParent<Rigidbody>();
            
            if (rb != null)
            {
                rb.linearDamping = 20f; // Apply slow effect
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log("Exited trigger: " + other.name);
            Rigidbody rb = other.GetComponentInParent<Rigidbody>();
           
            if (rb != null)
            {
                StartCoroutine(ResetDrag(rb)); // Reset drag (slow effect) to 0
            }
        }

        /// <summary>
        /// Resets the drag (slow effect) 
        /// </summary>
        private IEnumerator ResetDrag(Rigidbody rb)
        {
            yield return null;

            if (rb != null)
            {
                rb.linearDamping = 0f; // Reset to default value
            }
        }
    }
}
