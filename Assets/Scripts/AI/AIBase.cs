using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using AIAnimation;
using Defender;
using Unity.Netcode;

/// <summary>
/// Shared basic state machine logic/rules used by all AIs. Covers common refs; how to change state and shared rotation logic.
/// </summary>
public class AIBase : CharacterBase
{
   [Header("References")]
    public NavMeshAgent agent; // Movement agent
    private Health health; //  health system
    private Transform player; // Player reference (can be assigned later)

    [Header("Patrolling")]
    public Transform[] patrolPoints; // Patrol points
    public int patrolPointsCount = 3; // Number of patrol points to generate/get

    [Header("Movement Settings")] // These are NavMesh agent settings
    [SerializeField] private float rotationSpeed = 5f; 
    [SerializeField] private float acceleration = 8f;  
    public float followDistance = 10f;

    [Header("Civ Params")] 
    public bool IsAbducted;
    [HideInInspector] public AlienAI escortingAlien;

    public void SetAbducted(bool abducted)
    {
        IsAbducted = abducted;
    }

    protected IAIState currentState; // A reference to current AI state (using interface)
    public IAIState CurrentState
    {
	    get
	    {
		    return currentState;
	    }
    }

    // Properties
    public NavMeshAgent Agent => agent;
    public Transform Player => player;
    public Transform[] PatrolPoints => patrolPoints;
    public float FollowDistance => followDistance;


    // Initialize AI - getting references
    protected virtual void Start()
    {
        health = GetComponent<AIHealth>();
        agent = GetComponent<NavMeshAgent>();
        patrolPoints = WaypointManager.Instance.GetUniqueWaypoints(patrolPointsCount);
        // Subscribe to health events
        health.OnHealthChanged += HandleHit;
        health.OnDeath += HandleDeath;
    }

    // State machine update loop
    protected virtual void Update()
    {
        CurrentState?.Stay();
    }

    // Change state logic
    public void ChangeState(IAIState newState)
    {
        CurrentState?.Exit();
        currentState = newState;
        CurrentState.Enter();
    }

    // Move AI to a target position smoothly
    public void MoveTo(Vector3 destination)
    {
        agent.acceleration = acceleration;
        agent.SetDestination(destination);
    }

    // Stop movement
    public void StopMoving()
    {
        if (!agent.isStopped)
            agent.isStopped = true;
    }

    // Resume movement
    public void ResumeMoving()
    {
        agent.isStopped = false;
    }

    // Health change callback
    private void HandleHit(float amount)
    {
        if (health.currentHealth > 0)
        {
            ChangeState(new HitState(this, CurrentState)); // Switch to Hit state
        }
    }

    // Death callback
    private void HandleDeath()
    {
        ChangeState(new DeathState(this)); // Switch to Death state
    }

    // Cleanup on destroy
    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnHealthChanged -= HandleHit;
            health.OnDeath -= HandleDeath;
        }
    }
    
    public void StartSuckUp(float height = 5f, float duration = 5f)
    {
        Debug.Log("[DropZone] Civilian entered zone, starting suck up!");
        StartCoroutine(SuckUpRoutine(height, duration));
    }

    private IEnumerator SuckUpRoutine(float height, float duration)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * height;

        // disable NavmeshAgent and any active ai states
        var agent = GetComponent<NavMeshAgent>();
        if (agent) agent.enabled = false;
        // play sound effect here?
        while (elapsed < duration)
        {
            float time = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, endPos, time);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
        // play a scream sound here...?
        // or pool
        Destroy(gameObject);
    }

}
