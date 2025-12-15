using UnityEngine;
using UnityEngine.AI;

namespace Shell_AI
{
    public enum AlienWaitState
    {
        Waiting,
        Attacking,
        Returning
    }

    public class AlienWait : MonoBehaviour
    {
        public AlienWaitState state = AlienWaitState.Waiting;

        private NavMeshAgent agent;
        private GameObject target;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        void Start()
        {
            AlienWaitCoordinator.Instance.RegisterAlien(this);
        }

        void Update()
        {
            if (state == AlienWaitState.Attacking && target != null)
            {
                agent.SetDestination(target.transform.position);
            }
            else if (state == AlienWaitState.Returning)
            {
                agent.SetDestination(
                    AlienWaitCoordinator.Instance.mothership.position
                );
            }
        }

        public void StartAttack(GameObject attackTarget)
        {
            target = attackTarget;
            state = AlienWaitState.Attacking;
        }

        void OnTriggerEnter(Collider other)
        {
            if (state != AlienWaitState.Attacking) return;

            if (other.CompareTag("Civilian") || other.CompareTag("Player"))
            {
                // Attack / capture logic goes here
                ReturnToMothership();
            }
        }

        void ReturnToMothership()
        {
            state = AlienWaitState.Returning;
            AlienWaitCoordinator.Instance.ResetAttack();
        }
    }
}
