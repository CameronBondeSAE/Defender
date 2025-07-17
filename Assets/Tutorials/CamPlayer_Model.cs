using System;
using UnityEngine;

namespace Tutorials
{
    public class CamPlayer_Model : MonoBehaviour
    {
        public int howManyThingsHaveIPickupUp = 0;

        // Activate items on touch
        private void OnCollisionEnter(Collision other)
        {
	        // Check for specific GO (REALLY BAD)
	        // Check for specific tag (Slightly better)
	        // Check for a specific script (Slightly better again)
	        // Check for a specific interface (MUCH better + we can actually interact with whatever)
	        if (other.gameObject.GetComponent<IInteractable>() != null)
	        {
		        // Activate Item action
		        other.gameObject.GetComponent<IInteractable>().Interact();
	        }
        }

        private void OnCollisionExit(Collision other)
        {
	        if (other.gameObject.GetComponent<IInteractable>() != null)
	        {
		        // Activate Item action
		        other.gameObject.GetComponent<IInteractable>().StopInteracting();
	        }
        }

        
        
        
        
        

        public CamPlayer_Controller CamPlayerController;
        public Rigidbody rb;
        public float speed = 100f;

        [SerializeField] private float jumpForce = 100f;

		
        private void FixedUpdate()
        {
            rb.AddForce(CamPlayerController.direction * speed);
        }

        public void Jump()
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }
    }
}