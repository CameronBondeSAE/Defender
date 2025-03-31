using UnityEngine;

public class ChaseState : IAlienState
{
    public void EnterState(AlienStateMachine alien) { }

    public void UpdateState(AlienStateMachine alien)
    {
        Vector3 direction = (alien.player.position - alien.transform.position).normalized;
        alien.rb.velocity = direction * alien.moveSpeed;
    }

    public void ExitState() { }
}
