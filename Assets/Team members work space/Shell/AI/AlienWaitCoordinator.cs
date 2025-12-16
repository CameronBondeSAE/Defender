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

        private List<AlienWait> waitingAliens = new();
        private bool attackInProgress;

        private AlienGPTService gpt;

        void Awake()
        {
            Instance = this;
            gpt = GetComponent<AlienGPTService>();
        }

        public void RegisterAlien(AlienWait alien)
        {
            if (attackInProgress) return;
            if (!waitingAliens.Contains(alien))
                waitingAliens.Add(alien);

            TryStartAttack();
        }

        async void TryStartAttack()
        {
            if (waitingAliens.Count < requiredAliens) return;

            string context =
                $"Aliens nearby: {waitingAliens.Count}\n" +
                $"Cooling down: {AnyCoolingDown()}";

            GPTDecision decision =
                await gpt.QueryDecisionAsync(context);

            if (decision != GPTDecision.Attack) return;

            GameObject target = FindTarget();
            if (target == null) return;

            attackInProgress = true;

            foreach (AlienWait alien in waitingAliens)
                alien.StartAttack(target);

            waitingAliens.Clear();
        }

        bool AnyCoolingDown()
        {
            foreach (var alien in waitingAliens)
            {
                var cd = alien.GetComponent<AlienCooldown>();
                if (cd != null && cd.IsCoolingDown)
                    return true;
            }
            return false;
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
