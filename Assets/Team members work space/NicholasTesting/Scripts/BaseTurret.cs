using Unity.Netcode;
using UnityEngine;



namespace NicholasScripts
{
    /// <summary>
    /// Base class for turrets: acquires targets, aims, gates fire; subclasses implement actual firing.
    /// </summary>
    public abstract class BaseTurret : NetworkBehaviour, IPowerable
    {
        /// <summary>
        /// Old Monobehaviour Code
        /// </summary>
        // [Header("MVC")]
        // [SerializeField] protected Model_Turret model;
        // [SerializeField] protected View_Turret view;
        //
        // [Header("Targeting")]
        // [SerializeField] private Transform turretRotator;
        // [SerializeField] private float rotationSpeed = 5f;
        // [SerializeField] private LayerMask raycastMask = Physics.DefaultRaycastLayers; // first-hit check
        //
        // private Transform currentTarget;
        //
        // private void Update()
        // {
        //     if (model == null || view == null) return;
        //
        //     // Acquire a valid target only if activated
        //     currentTarget = model.isActivated ? FindBestTarget() : null;
        //
        //     if (currentTarget != null)
        //     {
        //         RotateToward(currentTarget);
        //     }
        //
        //     // Timers & fire gate
        //     model.UpdateTimer(Time.deltaTime);
        //     if (model.isActivated && currentTarget != null && model.CanFire())
        //     {
        //         // Check LOS before committing a shot
        //         if (HasLineOfSight(currentTarget))
        //         {
        //             model.ResetTimer();
        //             Fire();
        //         }
        //     }
        // }
        //
        // protected abstract void Fire();
        //
        // private void RotateToward(Transform target)
        // {
        //     if (turretRotator == null || target == null) return;
        //
        //     Vector3 dir = (target.position - turretRotator.position);
        //     dir.y = 0f; 
        //     if (dir.sqrMagnitude < 0.0001f) return;
        //
        //     Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        //     turretRotator.rotation = Quaternion.Slerp(
        //         turretRotator.rotation, targetRot, rotationSpeed * Time.deltaTime
        //     );
        // }
        //
        // private Transform FindBestTarget()
        // {
        //     GameObject[] aliens = GameObject.FindGameObjectsWithTag("Alien");
        //     if (aliens.Length == 0) return null;
        //
        //     Transform best = null;
        //     float bestDist = float.MaxValue;
        //     Vector3 origin = view.GetFirePoint() != null ? view.GetFirePoint().position : transform.position;
        //
        //     foreach (var alienGO in aliens)
        //     {
        //         Transform t = alienGO.transform;
        //         float dist = Vector3.Distance(origin, t.position);
        //         if (dist > model.range) continue; // out of range
        //
        //         if (!HasLineOfSight(t)) continue; // blocked by wall/obstacle
        //
        //         if (dist < bestDist)
        //         {
        //             best = t;
        //             bestDist = dist;
        //         }
        //     }
        //
        //     return best; 
        // }
        //
        // private bool HasLineOfSight(Transform target)
        // {
        //     if (target == null) return false;
        //
        //     Vector3 origin = view.GetFirePoint() != null ? view.GetFirePoint().position : transform.position;
        //     Vector3 toTarget = target.position - origin;
        //     float maxDist = Mathf.Min(model.range, toTarget.magnitude + 0.01f);
        //     Vector3 dir = toTarget.normalized;
        //
        //     if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDist, raycastMask, QueryTriggerInteraction.Ignore))
        //     {
        //         return hit.transform.CompareTag("Alien");
        //     }
        //
        //     return false;
        // }
        //
        // public void SetPowered(bool state)
        // {
        //     model.isPowered = state;
        // }
        
        [Header("MVC")]
    [SerializeField] protected Model_Turret model;   // Inherits UsableItem_Base (NetworkBehaviour)
    [SerializeField] protected View_Turret view;

    [Header("Targeting")]
    [SerializeField] private Transform turretRotator;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private LayerMask raycastMask = Physics.DefaultRaycastLayers;

    // Server writes, everyone reads
    private readonly NetworkVariable<Quaternion> turretRotation =
        new NetworkVariable<Quaternion>(
            Quaternion.identity,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private Transform currentTarget;

    private void Update()
    {
        if (!IsServer) return;
        if (model == null || view == null) return;
        currentTarget = model.isActivated ? FindBestTarget() : null;

        if (currentTarget != null)
        {
            RotateToward(currentTarget);
        }

        model.UpdateTimer(Time.deltaTime);
        if (model.isActivated && currentTarget != null && model.CanFire())
        {
            if (HasLineOfSight(currentTarget))
            {
                model.ResetTimer();
                Fire();              
            }
        }
    }

    private void LateUpdate()
    {
        if (!IsServer && turretRotator != null)
        {
            turretRotator.rotation = turretRotation.Value; // client shows server rotation
        }
    }

    protected abstract void Fire();

    private void RotateToward(Transform target)
    {
        if (turretRotator == null || target == null) return;

        Vector3 dir = target.position - turretRotator.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
        turretRotator.rotation = Quaternion.Slerp(
            turretRotator.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );

        turretRotation.Value = turretRotator.rotation; // server auth
    }

    private Transform FindBestTarget()
    {
        GameObject[] aliens = GameObject.FindGameObjectsWithTag("Alien");
        if (aliens == null || aliens.Length == 0) return null;

        Transform best = null;
        float bestDist = float.MaxValue;

        Vector3 origin = (view != null && view.GetFirePoint() != null)
            ? view.GetFirePoint().position
            : transform.position;

        int i = 0;
        while (i < aliens.Length)
        {
            GameObject alienGO = aliens[i];
            Transform t = alienGO != null ? alienGO.transform : null;

            if (t != null)
            {
                float dist = Vector3.Distance(origin, t.position);
                if (dist <= model.range)
                {
                    if (HasLineOfSight(t))
                    {
                        if (dist < bestDist)
                        {
                            best = t;
                            bestDist = dist;
                        }
                    }
                }
            }

            i = i + 1;
        }

        return best;
    }

    private bool HasLineOfSight(Transform target)
    {
        if (target == null) return false;

        Vector3 origin = (view != null && view.GetFirePoint() != null)
            ? view.GetFirePoint().position
            : transform.position;

        Vector3 toTarget = target.position - origin;
        float maxDist = Mathf.Min(model.range, toTarget.magnitude + 0.01f);
        Vector3 dir = toTarget.normalized;

        RaycastHit hit;
        if (Physics.Raycast(origin, dir, out hit, maxDist, raycastMask, QueryTriggerInteraction.Ignore))
        {
            return hit.transform != null && hit.transform.CompareTag("Alien");
        }

        return false;
    }

    public void SetPowered(bool state)
    {
        model.isPowered = state;
    }
    
    }
}