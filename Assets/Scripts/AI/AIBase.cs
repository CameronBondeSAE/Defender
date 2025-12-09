using System;
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

	private Health    health; //  health system
	private Transform player; // Player reference (can be assigned later)

	[Header("Patrolling")]
	public Transform[] patrolPoints; // Patrol points

	public int patrolPointsCount = 3; // Number of patrol points to generate/get

	[Header("Movement Settings")]
	// These are NavMesh agent settings
	[SerializeField]
	private float rotationSpeed = 5f;

	[SerializeField]
	private float maxSpeed = 8f;

	public float forwardForce = 50f;

	public float followDistance = 10f;

	[Header("Civ Params")]
	public bool IsAbducted;
	private bool isBeingSucked;
	private bool isCivilian = true;

	[HideInInspector]
	public AlienAI escortingAlien;

	public bool useRigidbody = true;

	public Rigidbody rb;
	
	public NavMeshObstacle navMeshObstacle;
	
	[Header("To expose to statemachine")]
	public NavMeshPath Path => path;
	public int CornerIndex => cornerIndex;
	public bool UseRigidbody => useRigidbody;
	
	[Header("Debug")]
	[Tooltip( "Show path lines etc" )]
	public bool debug = true;

	public void SetAbducted(bool abducted)
	{
		IsAbducted = abducted;
	}

	protected IAIState currentState; // A reference to current AI state (using interface)
	private IAIState previousState;

	public IAIState CurrentState
	{
		get
		{
			return currentState;
		}
	}

	// Properties
	public NavMeshAgent Agent
	{
		get
		{
			if (agent != null) return agent;
			return null;
		}
	}

	public NavMeshPath        path;
	public Transform   Player         => player;
	public Transform[] PatrolPoints   => patrolPoints;
	public float       FollowDistance => followDistance;

	public float cornerThreshold = 0.75f;
	public int   cornerIndex;

	public  float   stuckTime;
	public  float   maxStuckTime = 2f;
	private Vector3 lastDestination;
		
	// Initialize AI - getting references
	protected virtual void Start()
	{
		if(!IsServer)
			return;
			
		health = GetComponent<AIHealth>();
		agent  = GetComponent<NavMeshAgent>();
		if (useRigidbody)
		{
			if (!rb) rb = GetComponent<Rigidbody>();
			if (!rb)
			{
				rb = gameObject.AddComponent<Rigidbody>();
				rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
			}
			agent.enabled = false;
			path = new NavMeshPath();
		}

		patrolPoints = WaypointManager.Instance.GetUniqueWaypoints(patrolPointsCount);
		// Subscribe to health events
		health.OnHealthChanged += HandleHit;
		health.OnDeath         += HandleDeath;
	}

	// State machine update loop
	protected virtual void Update()
	{
		if(!IsServer)
			return;

		CurrentState?.Stay();
	}

	private void FixedUpdate()
	{
		if(!IsServer)
			return;
		
		if (useRigidbody)
		{
			if(path != null && path.corners.Length > 0 && cornerIndex < path.corners.Length)
			{
			
				AgentMovement();
				
				// Stuck detector. Recalculates path, eg if player shoves something in its way.
				UnStuck();
			}
		}
	}

<<<<<<< HEAD
=======
	public virtual void AgentMovement()
	{
		// Turn towards
		Vector3 direction   = path.corners[cornerIndex] - rb.position;
		float   angle = Vector3.SignedAngle(rb.transform.forward, direction, Vector3.up);
		rb.AddTorque(0, Mathf.Sign(angle) * rotationSpeed, 0);
		
		// Simple move forward with slowdown
		float slowDownScalar = (1f - Mathf.Abs(angle/180f)); // Normalise to 0-1
		if (rb.linearVelocity.magnitude < maxSpeed)
		{
			rb.AddRelativeForce(0,0,forwardForce * slowDownScalar * MoveSpeed);
		}
		
		Debug.DrawLine(path.corners[cornerIndex], path.corners[cornerIndex] + Vector3.up*10f, Color.red);
		// rb.MovePosition(Vector3.MoveTowards(rb.position, path.corners[cornerIndex], acceleration * Time.deltaTime));
		float distance = Vector3.Distance(rb.position, path.corners[cornerIndex]);
		// Debug.Log(gameObject.name + " : cornerThreshold = " + cornerThreshold+ " : distance = " + distance + " : cornerIndex = " + cornerIndex + " : path.corners.Length = " + path.corners.Length, gameObject);
		if (distance < cornerThreshold)
		{
			cornerIndex++;
			// Debug.Log("		" + gameObject.name + " : AIBase reached corner");
			if (cornerIndex >= path.corners.Length)
			{
				rb.linearVelocity = Vector3.zero;
				rb.angularVelocity = Vector3.zero;
				path = null;
				cornerIndex = 0;
				// Debug.Log(gameObject.name + " : AIBase Reached destination");
			}
		}
	}


	public virtual void UnStuck()
	{ 
		if (rb.linearVelocity.magnitude < 0.5f)
		{
			stuckTime += Time.fixedDeltaTime;
			if (stuckTime > maxStuckTime)
			{
				// Debug.Log("AI STUCK! : Recalculating path");
				MoveTo(lastDestination);
				stuckTime = 0;
			}
		}
    }


>>>>>>> main
	// Change state logic
	public void ChangeState(IAIState newState)
	{
		previousState = currentState;
		CurrentState?.Exit();
		currentState = newState;
		CurrentState.Enter();
	}

	
	public IEnumerator moveTo_Coroutine;

	// Move AI to a target position smoothly
	public void MoveTo(Vector3 destination)
	{
		if(!IsServer)
			return;

		// OLD CODE
		/*// Debug.Log("		Moving to " + destination);
		
		lastDestination = destination;
		
		// Otherwise the character is sitting in a dead zone in the carved navmesh
		// navMeshObstacle.enabled = false;
		
		// Debug.Log("Moving to " + destination);
		if (useRigidbody)
		{
			if (path != null) NavMesh.CalculatePath(transform.position, destination, NavMesh.AllAreas, path);
			cornerIndex = 0;
		}
		else if (agent != null && agent.enabled == true)
		{
			agent.acceleration = acceleration;
			agent.SetDestination(destination);
		}
		
		// navMeshObstacle.enabled = true;
		// return;
		
		if(moveTo_Coroutine != null)
			StopCoroutine(moveTo_Coroutine);
		
		moveTo_Coroutine = CalculatePath_Coroutine(destination);
		StartCoroutine(moveTo_Coroutine);*/
		
		// Debug.Log("Moving to " + destination);
		lastDestination = destination;

		if (useRigidbody)
		{
			// Find nearest valid NavMesh position for the AI
			NavMeshHit startHit;
			// Vector3 startPos = transform.position + transform.forward * 0.5f;
			Vector3 startPos = transform.position;
			if (!NavMesh.SamplePosition(startPos, out startHit, 10f, NavMesh.AllAreas))
			{
				Debug.LogWarning($"[MoveTo] {name} cannot find valid NavMesh start position near {startPos}");
				return;
			}

			// find nearest valid navmesh pos for destination
			NavMeshHit destHit;
			if (!NavMesh.SamplePosition(destination, out destHit, 10f, NavMesh.AllAreas))
			{
				return;
			}

			// get path between these points
			if (path == null) path = new NavMeshPath();
        
			bool pathFound = NavMesh.CalculatePath(startHit.position, destHit.position, NavMesh.AllAreas, path);
        
			if (pathFound && path.status == NavMeshPathStatus.PathComplete)
			{
				cornerIndex = 1;
			}
			else
			{
				path = null;
			}
		}
		else if (agent != null && agent.enabled == true)
		{
			agent.acceleration = maxSpeed;
			agent.SetDestination(destination);
		}
	}

	// Stop movement
	public void StopMoving()
	{
		// TODO work with rb
		if (agent != null && agent.enabled == true && !agent.isStopped)
			agent.isStopped = true;
	}

	// Resume movement
	public void ResumeMoving()
	{
		// TODO work with rb
		if (agent != null && agent.enabled == true) agent.isStopped = false;
	}

	// Health change callback
	private void HandleHit(float amount)
	{
		if (health.currentHealth.Value > 0)
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
			health.OnDeath         -= HandleDeath;
		}
	}

	public void StartSuckUp(float height = 5f, float duration = 5f)
	{
		Debug.Log("[DropZone] Civilian entered zone, starting suck up!");
		if (!IsServer || isBeingSucked) return;
		isBeingSucked = true;
		// stops the civ first!
		if (useRigidbody && rb != null)
		{
			rb.linearVelocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			rb.isKinematic = true;  // cancles physics
		}

		// cancels cam's path, maybe will fix?
		path = null;
		cornerIndex = 0;
		StartCoroutine(SuckUpRoutine(height, duration));
	}

	private IEnumerator SuckUpRoutine(float height, float duration)
	{
		if(!IsServer) yield break;

		Debug.Log("being sucked now");
		float   elapsed  = 0f;
		Vector3 startPos = transform.position;
		Vector3 endPos   = startPos + Vector3.up * height;

		// disable NavmeshAgent and any active ai states
		var agent                = GetComponent<NavMeshAgent>();
		if (agent) agent.enabled = false;
		// play sound effect here?
		while (elapsed < duration)
		{
			float time = elapsed / duration;
			transform.position =  Vector3.Lerp(startPos, endPos, time);
			elapsed            += Time.deltaTime;
			yield return null;
		}

		transform.position = endPos;
		// play a scream sound here...?
		// or pool
		AIHealth aiHealth = GetComponent<AIHealth>() ?? GetComponentInChildren<AIHealth>();
		if (aiHealth != null)
		{
			aiHealth.Kill(); 
		}
		else
		{
			Debug.LogWarning($"AIHealth null during suck-up.");
		}
		//Destroy(this.gameObject);
	}
	
	public virtual IAIState GetDefaultState()
	{
		return previousState ?? currentState;
	}
	public void ClearPreviousState() // ok this is added for when we DONT want to return to previous state
	{
		previousState = null;
	}

	private void OnDrawGizmos()
	{
		if(!IsServer)
			return;

		if(debug == false || path == null || path.corners.Length<=0) return;
		
		for (int i = 0; i < path.corners.Length; i++)
		{
			if(cornerIndex == i)
				Gizmos.color = Color.green;
			else
			{
				Gizmos.color = Color.white;
			}
			Gizmos.DrawSphere(path.corners[i], 0.2f);
			if(i>0)
				Gizmos.DrawLine(path.corners[i], path.corners[i - 1]);
		}
	}
}