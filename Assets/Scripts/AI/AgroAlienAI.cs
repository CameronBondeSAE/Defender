using UnityEngine;
using AIAnimation;

public class AgroAlienAI : AIBase
{
   [Header("Attack Damage")]
    public float damageMin = 5f;
    public float damageMax = 10f;

    [Header("Detection")]
    public float alertRange = 10f;
    public float fieldOfViewAngle = 90f; // fov angle
    public int rayCount = 5;
    public LayerMask detectionMask;

    [HideInInspector] public Transform detectedPlayer;

    private AIAnimationController animController;
    private PatrolState patrolState;
    private AttackState attackState;
    

    protected override void Start()
    {
        base.Start();
        animController = GetComponentInChildren<AIAnimationController>();
        patrolState = new PatrolState(this);
        attackState = new AttackState(this);
        ChangeState(patrolState);
    }

    void Update()
    {
        base.Update();
       // Only scan for player while patrolling...?
        if (currentState == patrolState)
        {
            if (ScanForPlayer())
            {
                ChangeState(attackState);
            }
        }
    }

    // FoV code
    public bool ScanForPlayer()
    {
        Vector3 origin = transform.position + Vector3.up * 1.1f; // Slightly above ground
        float halfAngle = fieldOfViewAngle / 2f;

        for (int i = 0; i < rayCount; i++)
        {
            float lerpFactor = (float)i / (rayCount - 1);
            float angle = Mathf.Lerp(-halfAngle, halfAngle, lerpFactor);
            Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 direction = rotation * transform.forward;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, alertRange, detectionMask))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    detectedPlayer = hit.transform;
                    Debug.DrawRay(origin, direction * hit.distance, Color.red); // if sees player
                    return true;
                }
            }

            Debug.DrawRay(origin, direction * alertRange, Color.yellow); // visual debugging
        }

        return false;
    }
}
