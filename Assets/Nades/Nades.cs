using System.Collections;
using UnityEngine;

namespace Brad
{
    public class Nades : MonoBehaviour
    {
        public GrenadeTrajectory grenadeTrajectory; // Reference to a ScriptableObject that stores line renderer points
        protected Rigidbody rb;
        protected bool hasLaunched = false; // Has the grenade been launched
        protected bool hasLanded = false; // has the grenade hit the ground
        protected bool hasCollided = false; // Has the grenade collided with anything

        // Store the trajectory values at the time of launch
        private float storedLaunchForce; // Force applied at launch
        private float storedTimeBetweenPoints; // Time between each trajectory point
        private int storedArcPoints; // Total number of arc points
        private Vector3[] storedCalculatedPoints; // Trajectory points array

        private Coroutine trajectoryCoroutine; // Reference to the trajectory coroutine

        public virtual void Start()
        {
            rb = GetComponent<Rigidbody>(); // Get the rigidbody 

            // Initialize values from the ScriptableObject
            if (grenadeTrajectory != null)
            {
                storedLaunchForce = grenadeTrajectory.launchForce;
                storedTimeBetweenPoints = grenadeTrajectory.timeBetweenPoints;
                storedArcPoints = grenadeTrajectory.arcPoints;

                // Copy the trajectory point array
                storedCalculatedPoints = new Vector3[grenadeTrajectory.calculatedPoints.Length];
                grenadeTrajectory.calculatedPoints.CopyTo(storedCalculatedPoints, 0);
            }
            else
            {
                Debug.LogWarning("GrenadeTrajectory not assigned!");
            }
        }

        /// <summary>
        /// Launches the grenade along a given direction
        /// </summary>
        public void Launch(Vector3 launchDirection)
        {
            if (grenadeTrajectory == null || storedCalculatedPoints == null || storedCalculatedPoints.Length < 2)
            {
                Debug.LogWarning("GrenadeTrajectory is invalid or not enough trajectory points.");
                return;
            }

            if (!hasLaunched)
            {
                hasLaunched = true;

                // Stop any existing trajectory coroutine
                if (trajectoryCoroutine != null)
                {
                    StopCoroutine(trajectoryCoroutine);
                }

                // Launch grenade and follow the trajectory
                trajectoryCoroutine = StartCoroutine(FollowTrajectory(launchDirection));
            }
        }

        /// <summary>
        /// Coroutine to move the grenade through the pre-calculated trajectory points 
        /// </summary>
        private IEnumerator FollowTrajectory(Vector3 launchDirection)
        {
            int currentPoint = 0; // Start at the first point
            Vector3 startPos = transform.position; // Initial position of the grenade

            // Loop through all trajectory points unless a collision occurs
            while (currentPoint < storedCalculatedPoints.Length - 1 && !hasCollided)
            {
                Vector3 currentTarget = storedCalculatedPoints[currentPoint + 1]; // Target next point
                float travelTime = storedTimeBetweenPoints; // Duration to travel between points
                float elapsedTime = 0f; // Timer for interpolation


                // Interpolate position between current and next point
                while (elapsedTime < travelTime && !hasCollided)
                {
                    transform.position = Vector3.Lerp(startPos, currentTarget, elapsedTime / travelTime);
                    elapsedTime += Time.deltaTime; // Update time
                    yield return null;
                }

                transform.position = currentTarget; // Snap to the target point
                startPos = currentTarget; // Update starting point
                currentPoint++; // Move to the next arc point
            }

            trajectoryCoroutine = null; // Clear the reference once complete
        }

        /// <summary>
        /// Handles all collision logic for the grenade
        /// </summary>
        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Wall")) // If grenade hits a wall, apply bounce force
            {
                Vector3 wallBounce = transform.position - collision.transform.position; // Direction away from wall
                float forceMagnituder = 70f; // Bounce strength
                rb.AddForce(wallBounce.normalized * forceMagnituder, ForceMode.Impulse); // Apply bounce
            }

            // Ignore collision with player & alien
            if (collision.gameObject.CompareTag("Player")) //&& collision.gameObject.CompareTag("Alien"))
            {
                return;
            }


            // If grenade hits the ground, mark as landed and trigger logic
            if (collision.gameObject.CompareTag("Ground"))
            {
                hasLanded = true;
                GrenadeLanded(); // Trigger virtual function
            }
        }

        /// <summary>
        ///  // Virtual function that can be overridden by specific grenade types
        /// </summary>
        protected virtual void GrenadeLanded()
        {
            Debug.Log("Base class check - Grenade landed!");
        }

        /// <summary>
        ///  Clean up coroutine when the grenade is destroyed
        /// </summary>
        private void OnDestroy()
        {
            // Ensure coroutines are stopped when the object is destroyed
            if (trajectoryCoroutine != null)
            {
                StopCoroutine(trajectoryCoroutine);
            }
        }
    }
}