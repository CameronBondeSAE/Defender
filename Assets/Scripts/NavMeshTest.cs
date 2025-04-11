using System.Collections.Generic;
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
        //SetNextTarget();
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
        //Transform nextTarget = allCivs[Random.Range(0, allCivs.Count)];
        foreach (Transform civ in allCivs)
        {
            float dist = Vector3.Distance(transform.position, civ.position);
            if (dist < distance)
            {
                distance = dist;
                closest = civ;
            }
        }
        currentTarget = closest;
        agent.SetDestination(closest.position);
    }
    
    public void FaceDirection()
    {
        if (agent.velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}
