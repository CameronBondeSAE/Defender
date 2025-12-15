using System.Collections.Generic;
using UnityEngine;

namespace Shell_AI
{
    public class AlienWaitCoordinator : MonoBehaviour
    {
        public static AlienWaitCoordinator Instance;

        [Header("Alien Wait Settings")]
        public int requiredAliens = 3;
        public Transform mothership;

        private List<AlienWait> waitingAliens = new List<AlienWait>();
        private bool attackStarted = false;

        void Awake()
        {
            Instance = this;
        }

        public void RegisterAlien(AlienWait alien)
        {
            if (attackStarted) return;

            if (!waitingAliens.Contains(alien))
            {
                waitingAliens.Add(alien);
            }

            TryStartAttack();
        }

        void TryStartAttack()
        {
            if (waitingAliens.Count < requiredAliens) return;

            GameObject target = FindTarget();
            if (target == null) return;

            attackStarted = true;

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
            attackStarted = false;
        }
    }
}
