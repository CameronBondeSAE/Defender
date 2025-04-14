using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using AIAnimation;

public class NavMeshTest : MonoBehaviour
{
   private NavMeshAgent agent;

    private List<Transform> allCivs = new List<Transform>();
    private List<Transform> collectedCivs = new List<Transform>();

    private float rotationSpeed = 5;

    public Transform mothership;
    private Transform currentTarget;
    public bool onWayToMothership = false;

    private AIAnimationController animController;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animController = GetComponent<AIAnimationController>();

        GameObject[] civilians = GameObject.FindGameObjectsWithTag("Civilian");
        foreach (GameObject civ in civilians)
        {
            allCivs.Add(civ.transform);
        }
    }

    void Update()
    {
        SetNextTarget();
        FaceDirection();
        animController.SetAnimation(AIAnimationController.AnimationState.Walk);
    }

    void SetNextTarget()
    {
        Transform closest = null;
        float distance = Mathf.Infinity;

        foreach (Transform civ in allCivs)
        {
            if (collectedCivs.Contains(civ)) continue; // Skip collected civs

            float dist = Vector3.Distance(transform.position, civ.position);
            if (dist < distance)
            {
                distance = dist;
                closest = civ;
            }
        }

        if (closest != null && closest != currentTarget)
        {
            currentTarget = closest;
            agent.SetDestination(closest.position);
        }
    }

    public void FaceDirection()
    {
        if (agent.velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Civilian"))
        {
            Transform civ = collision.transform;

            if (!collectedCivs.Contains(civ))
            {
                collectedCivs.Add(civ); // Mark as collected
            }

            StartCoroutine(ResumeAfterDelay());
        }
    }

    private IEnumerator ResumeAfterDelay()
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(3f);
        agent.isStopped = false;
    }
}
