using System;
using UnityEngine;
using System.Collections;

namespace Brad
{
    public class TimeNade : Nades
    {
        private float originalDrag = 0.0f;

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
            Debug.Log("Slow Grenade triggered.");

            transform.localScale *= 13f;
            rb.useGravity = false;
            rb.isKinematic = true;

            // Set the collider as a trigger
            Collider collider = GetComponentInChildren<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            // Destroy the grenade after 7 seconds
            Destroy(gameObject, 7f);
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"{other.name} entered the grenade trigger.");
            Rigidbody rb = other.GetComponentInParent<Rigidbody>();

            if (rb != null)
            {
                originalDrag = rb.linearDamping;
                rb.linearDamping = 10f; // Apply drag
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log($"{other.name} left the grenade trigger.");
            Rigidbody rb = other.GetComponentInParent<Rigidbody>();
            if (rb != null)
            {
               
                    rb.linearDamping = originalDrag;
                
            }
        }
    }
}
