using UnityEngine;
using System.Collections;


public class AlienStateMachine : MonoBehaviour
{
    private IAlienState currentState;
    public IdleState idleState = new IdleState();
    public PatrolState patrolState = new PatrolState();
    public ChaseState chaseState = new ChaseState();
    public AttackState attackState = new AttackState();
    
    public Transform player;
    public Transform[] waypoints;
    public int currentWaypointIndex = 0;
    
    public float detectionRange = 5f;
    public float attackRange = 1.5f;
    public float moveSpeed = 2f;
    public float attackCooldown = 1f;
    
    public Rigidbody rb { get; private set; }
    private bool canAttack = true;
    
    /// <summary>
    /// Behaviour functions
    /// </summary>
    public void StopMovement()
    {
        rb.velocity = Vector3.zero;
    }
    
    public void StartAttackCooldown()
    {
        StartCoroutine(AttackCooldownRoutine());
    }

    private IEnumerator AttackCooldownRoutine()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}
