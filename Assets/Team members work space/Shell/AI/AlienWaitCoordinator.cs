using System.Collections.Generic;
using UnityEngine;

namespace Shell_AI
{
    public class AlienWaitCoordinator : MonoBehaviour
    {
        public static AlienWaitCoordinator Instance;

        [Header("Group Settings")]
        public int requiredAliens = 1;
        public Transform mothership;

        [Header("GPT (Optional)")]
        public bool useGPT = false;

        private List<AlienWait> waitingAliens = new();
        private bool attackInProgress = false;

        private AlienGPTService gpt;

        void Awake()
        {
            Instance = this;
            gpt = GetComponent<AlienGPTService>();
        }

        public void RegisterAlien(AlienWait alien)
        {
            if (attackInProgress) return;
            if (waitingAliens.Contains(alien)) return;

            waitingAliens.Add(alien);
            Debug.Log($"Alien registered. Count = {waitingAliens.Count}");

            TryStartAttack();
        }

        async void TryStartAttack()
        {
            if (waitingAliens.Count < requiredAliens)
                return;

            GameObject target = FindTarget();
            if (target == null)
            {
                Debug.Log("No target found");
                return;
            }

            attackInProgress = true;

            if (useGPT && gpt != null)
            {
                Debug.Log("Calling GPT...");
                GPTDecision decision =
                    await gpt.QueryDecisionAsync(
                        $"Aliens waiting: {waitingAliens.Count}"
                    );

                Debug.Log("GPT decision: " + decision);

                if (decision != GPTDecision.Attack)
                {
                    attackInProgress = false;
                    return;
                }
            }
            else
            {
                Debug.Log("GPT bypassed → ATTACK");
            }

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

        public void ResetCoordinator()
        {
            attackInProgress = false;
        }
    }
}
