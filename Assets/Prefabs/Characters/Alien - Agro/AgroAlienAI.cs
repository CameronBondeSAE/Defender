using UnityEngine;
using AIAnimation;

public class AgroAlienAI : AIBase
{
    [Header("Attack Damage")] public float damageMin = 5f;
    public float damageMax = 10f;

    [Header("Detection")] public float alertRange = 10f;
    public float fieldOfViewAngle = 90f; // field of view angle in degrees
    public int rayCount = 5; // number of rays cast in FoV scan
    public LayerMask detectionMask; // layers that can be detected (should include the player layer)

    [HideInInspector] public Transform detectedPlayer; // stores detected player reference

    private AIAnimationController animController; // reference to animation controller
    private PatrolState patrolState; // patrol state instance
    private AttackState attackState; // attack state instance

// Called when AI starts
    protected override void Start()
    {
        base.Start();
        animController = GetComponent<AIAnimationController>();
        patrolState = new PatrolState(this); // create patrol state
        attackState = new AttackState(this); // create attack state
        ChangeState(patrolState); // start in patrol state
    }

// Runs every frame
    void Update()
    {
        base.Update();

        // Only scan for player while patrolling
        if (CurrentState == patrolState)
        {
            if (ScanForPlayerAndDamagables()) // check for player in FoV
            {
                ChangeState(attackState); // switch to attack state if player found
            }
        }
    }

// Scans the field of view using multiple rays
// Returns true if player is detected
    public bool ScanForPlayerAndDamagables()
    {
        Vector3 origin = transform.position + Vector3.up * 1.1f; // position rays slightly above ground
        float halfAngle = fieldOfViewAngle / 2f;

        for (int i = 0; i < rayCount; i++)
        {
            float lerpFactor = (float)i / (rayCount - 1); // evenly space rays
            float angle = Mathf.Lerp(-halfAngle, halfAngle, lerpFactor); // calculate angle for this ray
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f); // rotate direction by angle
            Vector3 direction = rotation * transform.forward;

            // Cast ray
            if (Physics.Raycast(origin, direction, out RaycastHit hit, alertRange, detectionMask))
            {
                if (hit.collider.CompareTag("Player")) // if player hit
                {
                    detectedPlayer = hit.transform;
                    Debug.DrawRay(origin, direction * hit.distance, Color.red); // draw red ray for hit
                    return true;
                }
            }

            Debug.DrawRay(origin, direction * alertRange, Color.yellow); // draw yellow ray for no hit
        }

        return false;
    }
}
