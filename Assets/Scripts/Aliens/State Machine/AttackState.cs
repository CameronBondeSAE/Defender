using UnityEngine;

public class AttackState : MonoBehaviour
{
    public void EnterState(AlienStateMachine alien)
    {
        alien.StopMovement();
        alien.StartAttackCooldown();
    }

    public void UpdateState() { }

    public void ExitState() { }
}
