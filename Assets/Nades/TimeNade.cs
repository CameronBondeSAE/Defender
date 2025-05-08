using System.Collections;
using UnityEngine;

namespace Brad
{
    public class TimeNade : Nades
    {
        private Coroutine MoveAndDelete; // Moves the time grenade activating on trigger exit then deletes 

        /// <summary>
        /// Called when the grenade is instantiated
        /// </summary>
        public override void Start()
        {
            base.Start(); // Call base Nades class Start() to initialize trajectory values

            if (!hasLaunched) // If grenade hasn't been launched, launch it forward
            {
                Vector3 launchDirection = transform.forward; // Forward direction of this GameObject
                Launch(launchDirection);
            }
        }

        /// <summary>
        /// Called when the grenade lands
        /// </summary>
        protected override void GrenadeLanded()
        {
            Debug.Log("Slow Grenade triggered.");


            transform.localScale *= 11f; // Increase the size of the grenade  
            rb.useGravity = false; // Disable gravity so it stays floating
            rb.isKinematic = true; // Disable physics movement

            Collider collider = GetComponentInChildren<Collider>(); // Get the grenade collider 
            if (collider != null)
            {
                collider.isTrigger = true; // Make it a trigger zone
            }

            if (MoveAndDelete != null)
            {
                StopCoroutine(MoveAndDelete); // Stop any existing Coroutine 
            }

            // Start a coroutine to delete the grenade after its effect
            MoveAndDelete = StartCoroutine(DeleteTimeGrenade());
        }

        /// <summary>
        /// Steps to delete the time grenade and force OnTriggerExit
        /// </summary>
        /// <returns></returns>
        private IEnumerator DeleteTimeGrenade()
        {
            yield return new WaitForSeconds(3.0f); // Grenade duration 

            transform.localPosition = new Vector3(0.0f, -80f, 0f); // Drops then grenade to trigger OnTriggerExit

            yield return new WaitForSeconds(3.0f); // Delay just to make sure it worked


            Debug.Log("Delete time grenade.");
            Destroy(this.gameObject);
        }

        /// <summary>
        /// Called when something enters the grenade's trigger zone
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Entered trigger: " + other.name);
            Rigidbody rb = other.GetComponentInParent<Rigidbody>(); // Find the rigidbody of what entered 

            if (rb != null)
            {
                rb.linearDamping = 20f; // Apply drag to slow 
            }
        }

        /// <summary>
        /// Called when something exits the grenade's trigger zone
        /// </summary>
        private void OnTriggerExit(Collider other)
        {
            Debug.Log("Exited trigger: " + other.name);
            Rigidbody rb = other.GetComponentInParent<Rigidbody>(); // Get Rigidbody again

            if (rb != null)
            {
                rb.linearDamping = 0f; // Reset drag to default value removing the slow effect 
            }
        }
    }
}