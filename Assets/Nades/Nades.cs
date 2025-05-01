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

        private Coroutine trajectoryCoroutine; // Reference to the trajectory coroutine

        public virtual void Start()
        {
            rb = GetComponent<Rigidbody>();

            // Initialize values from the ScriptableObject
            if (grenadeTrajectory != null)
            {
                storedLaunchForce = grenadeTrajectory.launchForce;
                storedTimeBetweenPoints = grenadeTrajectory.timeBetweenPoints;
                storedArcPoints = grenadeTrajectory.arcPoints;
                storedCalculatedPoints = new Vector3[grenadeTrajectory.calculatedPoints.Length];
                grenadeTrajectory.calculatedPoints.CopyTo(storedCalculatedPoints, 0);
            }
            else
            {
                Debug.LogWarning("GrenadeTrajectory not assigned!");
            }
        }

        // public virtual void FixedUpdate()
        // {
        //     if (coll)
        // }

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

        private IEnumerator FollowTrajectory(Vector3 launchDirection)
        {
            int currentPoint = 0;
            Vector3 startPos = transform.position;

            // Follow the trajectory points
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

            // Enable gravity after the trajectory is completed
            rb.useGravity = true;
            trajectoryCoroutine = null; // Clear the reference
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Wall"))
            {
                Vector3 wallBounce = transform.position - collision.transform.position;
                float forceMagnituder = 70f;
                rb.AddForce(wallBounce.normalized * forceMagnituder, ForceMode.Impulse);
            }

            if (collision.gameObject.CompareTag("Player"))
            {
                return; // Ignore collision with Player
            }

            // Stop the trajectory when a collision occurs
            hasCollided = true;
            if (trajectoryCoroutine != null)
            {
                StopCoroutine(trajectoryCoroutine);
                trajectoryCoroutine = null;
            }

            if (collision.gameObject.CompareTag("Ground"))
            {
                hasLanded = true;
                GrenadeLanded();
            }
        }
        //     else
        //     {
        //         Debug.Log($"Grenade bounced off: {collision.gameObject.name}");
        //         StartCoroutine(DelayedLand());
        //     }
        // }
        //
        // private IEnumerator DelayedLand()
        // {
        //     yield return new WaitForSeconds(1f);
        //     GrenadeLanded();
        // }

        protected virtual void GrenadeLanded()
        {
            Debug.Log("Base class check - Grenade landed!");
        }

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

//         protected virtual void OnCollisionEnter(Collision collision)
//         {
//             if (hasLanded || hasCollided) return;
//
//             if (collision.gameObject.CompareTag("Player"))
//             {
//                 return; // Ignore collision with Player
//             }
//
//             // Stop the trajectory when a collision occurs
//             hasCollided = true;
//             if (trajectoryCoroutine != null)
//             {
//                 StopCoroutine(trajectoryCoroutine);
//                 trajectoryCoroutine = null;
//             }
//
//             if (collision.gameObject.CompareTag("Ground"))
//             {
//                 hasLanded = true;
//                 GrenadeLanded();
//             }
//             else
//             {
//                 Debug.Log($"Grenade bounced off: {collision.gameObject.name}");
//                 StartCoroutine(DelayedLand());
//             }
//         }
//
//         private IEnumerator DelayedLand()
//         {
//             yield return new WaitForSeconds(0.5f);
//             GrenadeLanded();
//         }
//
//         protected virtual void GrenadeLanded()
//         {
//             Debug.Log("Base class check - Grenade landed!");
//         }
//
//         private void OnDestroy()
//         {
//             // Ensure coroutines are stopped when the object is destroyed
//             if (trajectoryCoroutine != null)
//             {
//                 StopCoroutine(trajectoryCoroutine);
//             }
//         }
//     }
// }