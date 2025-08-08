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

        private void Update()
        {
            if (model == null || view == null) return;

            RotateTowardClosestAlien();

            model.UpdateTimer(Time.deltaTime);
            if (model.CanFire())
            {
                model.ResetTimer();
                Fire();
            }
        }

        protected abstract void Fire();

        protected void RotateTowardClosestAlien()
        {
            GameObject[] aliens = GameObject.FindGameObjectsWithTag("Alien");
            if (aliens.Length == 0) return;

            GameObject closest = null;
            float minDistance = float.MaxValue;

            foreach (var alien in aliens)
            {
                float dist = Vector3.Distance(transform.position, alien.transform.position);
                if (dist < minDistance)
                {
                    closest = alien;
                    minDistance = dist;
                }
            }

            if (closest != null)
            {
                Vector3 dir = (closest.transform.position - turretRotator.position).normalized;
                dir.y = 0; // keep level
                Quaternion targetRot = Quaternion.LookRotation(dir);
                turretRotator.rotation = Quaternion.Slerp(
                    turretRotator.rotation, targetRot, rotationSpeed * Time.deltaTime
                );
            }
        }

        public void SetPowered(bool state)
        {
            model.isPowered = state;
        }
    }
}