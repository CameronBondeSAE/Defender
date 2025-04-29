using System.Collections;
using UnityEngine;

namespace Brad
{
    public class Nades : MonoBehaviour
    {
        public GrenadeTrajectory grenadeTrajectory; // Reference to a ScriptableObject that stores line renderer points
        protected Rigidbody rb;
        protected bool hasLaunched = false; // Has the grenade been launched
        protected bool hasLanded = false;
        protected bool hasCollided = false; // Flag to track collision state

        // Store the trajectory values at the time of launch
        private float storedLaunchForce;
        private float storedTimeBetweenPoints;
        private int storedArcPoints;
        private Vector3[] storedCalculatedPoints;

        public virtual void Start()
        {
            rb = GetComponent<Rigidbody>();

            // Initial values for the scriptable objecta
            storedLaunchForce = grenadeTrajectory.launchForce; // Stores the launch force
            storedTimeBetweenPoints = grenadeTrajectory.timeBetweenPoints; // Stores the time between points
            storedArcPoints = grenadeTrajectory.arcPoints; // Stores the arc points
            storedCalculatedPoints = new Vector3[grenadeTrajectory.calculatedPoints.Length]; // Makes a copy of the calculated points
            grenadeTrajectory.calculatedPoints.CopyTo(storedCalculatedPoints, 0); // Copy trajectory points to an array
        }

        public void Launch(Vector3 launchDirection)
        {
            if (grenadeTrajectory == null)
            {
                Debug.LogWarning("GrenadeTrajectory not assigned!");
                return;
            }

            if (storedCalculatedPoints == null || storedCalculatedPoints.Length < 2)
            {
                Debug.LogWarning("Not enough trajectory points");
                return;
            }

            if (!hasLaunched)
            {
                hasLaunched = true;

                // Stop any existing trajectory coroutines
                StopCoroutine("FollowTrajectory");

                // Launch grenade and follow the trajectory
                StartCoroutine(FollowTrajectory(launchDirection));
            }
        }

        private IEnumerator FollowTrajectory(Vector3 launchDirection)
        {
            int currentPoint = 0;
            Vector3 startPos = transform.position;

            // Stop following the trajectory if collision happens
            while (currentPoint < storedCalculatedPoints.Length - 1 && !hasCollided)
            {
                Vector3 currentTarget = storedCalculatedPoints[currentPoint + 1];
                float travelTime = storedTimeBetweenPoints;
                float elapsedTime = 0f;

                while (elapsedTime < travelTime && !hasCollided)
                {
                    transform.position = Vector3.Lerp(startPos, currentTarget, elapsedTime / travelTime);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                
                transform.position = currentTarget;
                startPos = currentTarget;
                currentPoint++;
            }

            // When the grenade reaches the end of the trajectory or hits something
            rb.useGravity = true;
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (hasLanded || hasCollided) return;

          
            if (collision.gameObject.CompareTag("Player"))
            {
                return; // Ignore collision with Player
            }

            // Stop the trajectory following when collision occurs
            hasCollided = true;
            StopCoroutine("FollowTrajectory");
            
          
            if (collision.gameObject.CompareTag("Ground")) 
            {
                hasLanded = true;
                GrenadeLanded();
            }
            else
            {
               
                Debug.Log("Grenade bounced off: " + collision.gameObject.name);
                
            }
        }

        protected virtual void GrenadeLanded()
        {
            Debug.Log("Base class check - Grenade landed!");
          
        }
    }
}
