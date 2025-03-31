using UnityEngine;

public class IdleState : IAlienState
{
    public void EnterState(AlienStateMachine alien)
    {
        alien.StopMovement();
    }

    public void UpdateState()
    {
        // Remain idle until player or civilian enters detection range
    }

    public void ExitState() { }
}
