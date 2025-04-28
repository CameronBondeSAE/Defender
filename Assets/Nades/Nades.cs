using System.Collections;
using UnityEngine;

namespace Brad
{
    public class Nades : MonoBehaviour
    {
        public GrenadeTrajectory grenadeTrajectory; // Reference to a ScriptableObject that stores line renderer points
        protected Rigidbody rb;
        protected bool hasLaunched = false; // Has the grenade been launched

        // Store the trajectory values at the time of launch
        private float storedLaunchForce;
        private float storedTimeBetweenPoints;
        private int storedArcPoints;
        private Vector3[] storedCalculatedPoints;

        public virtual void Start()
        {
            rb = GetComponent<Rigidbody>(); 
           
            //initial values for the scriptable object
            storedLaunchForce = grenadeTrajectory.launchForce; // Stores the launch force

            storedTimeBetweenPoints = grenadeTrajectory.timeBetweenPoints; // Stores the time between points
           
            storedArcPoints = grenadeTrajectory.arcPoints; // Stores the arc points
           
            storedCalculatedPoints =
                new Vector3[grenadeTrajectory.calculatedPoints.Length]; // Makes a copy of the calculated points
          
            grenadeTrajectory.calculatedPoints.CopyTo(storedCalculatedPoints,
                0); // Copy trajectory points to an array
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
             
                // Immediately break out of the trajectory path
                StopCoroutine("FollowTrajectory"); // Stop all existing coroutines are stopped
               
                // Launch grenade and stop following the trajectory
                StartCoroutine(FollowTrajectory(launchDirection));
            }
        }

        private IEnumerator FollowTrajectory(Vector3 launchDirection)
        {
            int currentPoint = 0;
            Vector3 startPos = transform.position;
            
            while (currentPoint < storedCalculatedPoints.Length - 1)
            {
                Vector3 currentTarget = storedCalculatedPoints[currentPoint + 1];
                float travelTime = storedTimeBetweenPoints;

                
                float elapsedTime = 0f;
                while (elapsedTime < travelTime)
                {
                    transform.position = Vector3.Lerp(startPos, currentTarget, elapsedTime / travelTime);
                    elapsedTime += Time.deltaTime;
                    yield return null; 
                }

                
                transform.position = currentTarget;

                
                startPos = currentTarget;
                currentPoint++;
            }

            
            rb.useGravity = true;
        }
    }
}