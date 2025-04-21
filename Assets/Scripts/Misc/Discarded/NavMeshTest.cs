//using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using AIAnimation;
using Unity.VisualScripting;

public class NavMeshTest : MonoBehaviour
{
   private NavMeshAgent agent;
    private List<Transform> allCivs = new List<Transform>();
    private List<Transform> collectedCivs = new List<Transform>();
    private float rotationSpeed = 5f;

    public Transform mothership;
    private Transform currentTarget;
    public bool onWayToMothership = false;

    private AIAnimationController animController;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animController = GetComponent<AIAnimationController>();

        // Randomize avoidance priority to help prevent pathing deadlocks
        agent.avoidancePriority = Random.Range(30, 70);

        GameObject[] civilians = GameObject.FindGameObjectsWithTag("Civilian");
        foreach (GameObject civ in civilians)
        {
            // Avoid adding self to the list
            if (civ.transform != this.transform)
                allCivs.Add(civ.transform);
        }
    }

    void Update()
    {
        if (!agent.isStopped)
        {
            SetNextTarget();
            //FaceDirection();
            animController.SetAnimation(AIAnimationController.AnimationState.Walk);
        }
        if(onWayToMothership)
        {
            agent.SetDestination(mothership.position);
        }
    }

    void SetNextTarget()
    {
        if (currentTarget != null) return; // Already has a target

        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (Transform civ in allCivs)
        {
            if (collectedCivs.Contains(civ)) continue;

            float dist = Vector3.Distance(transform.position, civ.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = civ;
            }
        }

        if (closest != null)
        {
            currentTarget = closest;
            agent.SetDestination(currentTarget.position);
        }
    }

    void FaceDirection()
    {
        if (agent.velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Civilian"))
        {
            Transform civ = other.transform;

            if (!collectedCivs.Contains(civ))
            {
                collectedCivs.Add(civ); // Mark as collected

                // If the civilian has a CivAI component, tag it so it follows
                CivAI civAI = civ.GetComponent<CivAI>();
                if (civAI != null)
                {
                    //civAI.OnTagged(this.gameObject);
                }

                StartCoroutine(ResumeAfterDelay());
            }
        }
    }

    private IEnumerator ResumeAfterDelay()
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(2f);
        agent.isStopped = false;

        // Recalculate path in case previous destination is invalid
        if (currentTarget != null)
        {
            agent.SetDestination(currentTarget.position);
        }
    }
}
