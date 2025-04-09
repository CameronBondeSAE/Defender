using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshTest : MonoBehaviour
{
    private NavMeshAgent agent;
    
    private List<Transform> allCivs = new List<Transform>();
    private List<Transform> collectedCivs = new List<Transform>();

    public Transform mothership;
    private Transform currentTarget;
    public bool onWayToMothership = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
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
}
