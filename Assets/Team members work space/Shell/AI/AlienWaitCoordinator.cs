using System.Collections.Generic;
using UnityEngine;

namespace Shell_AI
{
    public class AlienWaitCoordinator : MonoBehaviour
    {
        public static AlienWaitCoordinator Instance;

        [Header("Group Settings")]
        public int requiredAliens = 3;
        public Transform mothership;

        private List<AlienWait> waitingAliens = new List<AlienWait>();
        private bool attackInProgress = false;

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void RegisterAlien(AlienWait alien)
        {
            if (attackInProgress) return;
            if (waitingAliens.Contains(alien)) return;

            waitingAliens.Add(alien);
            TryStartAttack();
        }

        void TryStartAttack()
        {
            if (waitingAliens.Count < requiredAliens) return;

            GameObject target = FindTarget();
            if (target == null) return;

            attackInProgress = true;

            foreach (AlienWait alien in waitingAliens)
            {
                alien.StartAttack(target);
            }

            waitingAliens.Clear();
        }

        GameObject FindTarget()
        {
            GameObject civ = GameObject.FindWithTag("Civilian");
            if (civ != null) return civ;

            return GameObject.FindWithTag("Player");
        }

        public void ResetAttack()
        {
            attackInProgress = false;
        }
    }
}
