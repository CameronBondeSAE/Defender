using UnityEngine;

namespace NicholasScripts
{
    public abstract class BaseTurret : MonoBehaviour, IPowerable
    {
        [Header("MVC")]
        [SerializeField] protected Model_Turret model;
        [SerializeField] protected View_Turret view;

        [Header("Targeting")]
        [SerializeField] private Transform turretRotator;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private LayerMask raycastMask = Physics.DefaultRaycastLayers; // first-hit check

        private Transform currentTarget;

        private void Update()
        {
            if (model == null || view == null) return;

            // Acquire a valid target only if activated
            currentTarget = model.isActivated ? FindBestTarget() : null;

            if (currentTarget != null)
            {
                RotateToward(currentTarget);
            }

            // Timers & fire gate
            model.UpdateTimer(Time.deltaTime);
            if (model.isActivated && currentTarget != null && model.CanFire())
            {
                // Check LOS before committing a shot
                if (HasLineOfSight(currentTarget))
                {
                    model.ResetTimer();
                    Fire();
                }
            }
        }

        protected abstract void Fire();

        private void RotateToward(Transform target)
        {
            if (turretRotator == null || target == null) return;

            Vector3 dir = (target.position - turretRotator.position);
            dir.y = 0f; 
            if (dir.sqrMagnitude < 0.0001f) return;

            Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
            turretRotator.rotation = Quaternion.Slerp(
                turretRotator.rotation, targetRot, rotationSpeed * Time.deltaTime
            );
        }

        private Transform FindBestTarget()
        {
            GameObject[] aliens = GameObject.FindGameObjectsWithTag("Alien");
            if (aliens.Length == 0) return null;

            Transform best = null;
            float bestDist = float.MaxValue;
            Vector3 origin = view.GetFirePoint() != null ? view.GetFirePoint().position : transform.position;

            foreach (var alienGO in aliens)
            {
                Transform t = alienGO.transform;
                float dist = Vector3.Distance(origin, t.position);
                if (dist > model.range) continue; // out of range

                if (!HasLineOfSight(t)) continue; // blocked by wall/obstacle

                if (dist < bestDist)
                {
                    best = t;
                    bestDist = dist;
                }
            }

            return best; 
        }

        private bool HasLineOfSight(Transform target)
        {
            if (target == null) return false;

            Vector3 origin = view.GetFirePoint() != null ? view.GetFirePoint().position : transform.position;
            Vector3 toTarget = target.position - origin;
            float maxDist = Mathf.Min(model.range, toTarget.magnitude + 0.01f);
            Vector3 dir = toTarget.normalized;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDist, raycastMask, QueryTriggerInteraction.Ignore))
            {
                return hit.transform.CompareTag("Alien");
            }

            return false;
        }

        public void SetPowered(bool state)
        {
            model.isPowered = state;
        }
    }
}