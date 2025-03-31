using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class PatrolState : IAlienState
{
    void EnterState(AlienStateMachine alien)
    {
        MoveToNextWaypoint(alien);
    }

    void UpdateState(AlienStateMachine alien)
    {
        Transform target = alien.waypoints[alien.currentWaypointIndex];

        Vector3 direction = (target.position - alien.transform.position).normalized;
        alien.rb.linearVelocity = direction * alien.moveSpeed;

        if (Vector3.Distance(alien.transform.position, target.position) < 1f)
        {
            alien.currentWaypointIndex = (alien.currentWaypointIndex + 1) % alien.waypoints.Length;
            MoveToNextWaypoint(alien);
        }
    }

    void ExitState() { }
    
    private void MoveToNextWaypoint(AlienStateMachine alien)
    {
        Transform nextWaypoint = alien.waypoints[alien.currentWaypointIndex];
        Vector3 direction = (nextWaypoint.position - alien.transform.position).normalized;
        alien.rb.linearVelocity = direction * alien.moveSpeed;
    }
}
