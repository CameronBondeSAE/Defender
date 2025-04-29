using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Shared basic state machine logic/rules (and nothing else) used by all AIs. Covers common refs; how to change state and shared rotation logic.
/// </summary>
public class AIBase : MonoBehaviour
{
   public NavMeshAgent agent;
   private Transform player; // all AI acknowledges the player
   protected IAIState currentState; // stores current state information
   // rotation smoothing params
   public float rotationSpeed;
   public float acceleration;
   public Transform[] patrolPoints;
   public int patrolPointsCount;
   private Health health;
   public float followDistance;
   
   // temp?
   protected bool isDead = false;

   protected virtual void Start()
   {
      health = GetComponent<AIHealth>();
      agent = GetComponent<NavMeshAgent>();
      patrolPoints = WaypointManager.Instance.GetUniqueWaypoints(patrolPointsCount);
      //patrolPoints = WaypointManager.Instance.GetAllWaypoints();
      //player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
      health.OnHealthChanged += HandleHit;
      health.OnDeath += HandleDeath;
   }

   // Exit current state and goes into new state
   public void ChangeState(IAIState newState)
   {
      if (currentState != null) currentState.Exit();
      currentState = newState;
      currentState.Enter();
   }

   // Runs Stay() for current states at runtime
   public void Update()
   {
      if (currentState != null) currentState.Stay();
   }
   
   // Makes sure AI always faces direction of movement
   public void FaceDirection()
   {
      if (agent.velocity.magnitude > 0.1f)
      {
         Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
         transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
      }
   }
   
   // Method that smoothly moves towards a target 
   public void MoveTo(Vector3 destination) {
      agent.acceleration = acceleration;
      agent.SetDestination(destination);
   }
   
   private void HandleHit(float amount)
   {
      if (health.currentHealth > 0)
      {
         ChangeState(new HitState(this, currentState)); // Switch to HitState if still alive
      }
   }

   private void HandleDeath()
   {
      ChangeState(new DeathState(this)); // Switch to DeathState when dead
   }

   private void OnDestroy()
   {
      if (health != null)
      {
         health.OnHealthChanged -= HandleHit;
         health.OnDeath -= HandleDeath;
      }
   }
   public void StopMoving() => agent.isStopped = true;
   public void ResumeMoving() => agent.isStopped = false;
}
