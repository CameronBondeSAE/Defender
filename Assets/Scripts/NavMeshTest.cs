using UnityEngine;
using UnityEngine.AI;

public class NavMeshTest : MonoBehaviour
{
    public Transform target;
    private NavMeshAgent agent;

    void Strat()
    {
        agent.GetComponent<NavMeshAgent>();
        agent.SetDestination(target.position);
    }
}
