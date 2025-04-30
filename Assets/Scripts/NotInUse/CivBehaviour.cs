using UnityEngine;
using System.Collections.Generic;
using AIAnimation;
using System.Collections;

/// <summary>
/// Civ AI using rb physics
/// </summary>
public class CivBehaviour : MonoBehaviour
{
  public List<Transform> waypoints;
    public float moveSpeed = 3f;
    public Rigidbody rb;
    public bool isTagged = false;
    public Transform followTarget;
    public Vector2 followOffset = new Vector2(-1f, 0);
    private int currentWaypointIndex;
    private AIAnimationController animController;

    private bool isWaitingToFollow = false;

    void Start()
    {
        animController = GetComponentInChildren<AIAnimationController>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = true;
    }

    void FixedUpdate()
    {
        FaceDirection();

        if (isTagged && followTarget != null && !isWaitingToFollow)
        {
            FollowAlien();
        }
        else if (!isTagged)
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;

        if (distance < 2f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            return;
        }

        direction.Normalize();
        rb.linearVelocity = direction * moveSpeed;
    }

    void FollowAlien()
    {
        Vector3 desiredPosition = followTarget.position + (Vector3)followOffset;
        Vector3 direction = desiredPosition - transform.position;

        if (direction.magnitude > 0.1f)
        {
            direction.Normalize();
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    public void StartFollowing(Transform alien)
    {
        followTarget = alien;
        isTagged = true;
        StartCoroutine(FollowDelay());
    }

    private IEnumerator FollowDelay()
    {
        isWaitingToFollow = true;
        rb.linearVelocity = Vector3.zero;
        yield return new WaitForSeconds(1f);
        isWaitingToFollow = false;
    }

    public void FaceDirection()
    {
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.linearVelocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);
            animController.SetAnimation(AIAnimationController.AnimationState.Walk);
        }
        else
        {
            animController.SetAnimation(AIAnimationController.AnimationState.Idle);
        }
    }
    }
    
    /// <summary>
    /// Civ AI script, now using a hybrid of rb physics and NavMesh. The other handles only static obstacle (eg. wall) avoidance.
    /// </summary>
    /*private Rigidbody rb;
    private NavMeshAgent agent;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private List<Transform> waypoints;
    private int currentWaypointIndex = 0;

    private AIAnimationController animController;
    
    private float repathRate = 0.5f;
    private float repathTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = true;

        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false; // Disable automatic movement
        agent.updateRotation = false;

        animController = GetComponent<AIAnimationController>();
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            agent.SetDestination(target.position);
            repathTimer = repathRate;
        }

        Vector3 toNextPos = (agent.nextPosition - transform.position);
        toNextPos.y = 0; // Ignore vertical movement

        if (toNextPos.magnitude < 1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            return;
        }

        // Move cow using Rigidbody
        Vector3 moveDirection = toNextPos.normalized;
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);

        // Rotate smoothly towards the movement direction
        if (moveDirection != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
        }

        // Animation
        animController.SetAnimation(AIAnimationController.AnimationState.Walk);

        // Sync NavMeshAgent with actual position to prevent drifting
        agent.nextPosition = transform.position;
        */
    

