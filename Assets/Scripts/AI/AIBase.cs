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
   
   // temp?
   protected bool isDead = false;

   protected virtual void Start()
   {
      agent = GetComponent<NavMeshAgent>();
      //player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
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

   // public void Die()
   // {
   //    if (isDead) return;
   //    if (!isDead)
   //    {
   //       isDead = true;
   //       ChangeState(new DeathState(this));
   //    }
   // }
}
