using System;
using UnityEngine;
using System.Collections;

namespace Brad
{
    public class TimeNade : Nades
    {
        private float originalDrag = 0.0f;
        private Coroutine exitTriggerCoroutine;

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

            transform.localScale *= 11f;
            rb.useGravity = false;
            rb.isKinematic = true;

            // Set the collider as a trigger
            Collider collider = GetComponentInChildren<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            //Drop & Destroy the grenade after 5 seconds
            if (exitTriggerCoroutine != null)
            {
                StopCoroutine(exitTriggerCoroutine);
            }
            
            exitTriggerCoroutine = StartCoroutine(DeleteTimeGrenade());
        }

        private IEnumerator DeleteTimeGrenade()
        {
          transform.localPosition = new Vector3(0.0f, transform.localPosition.y, transform.localPosition.z);
          Destroy(this.gameObject);
          yield return null;
          Debug.Log("Delete time grenade.");
        }
    }
}