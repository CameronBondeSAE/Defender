using UnityEngine;

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

        [Header("Movement")]
        public float moveSpeed = 3f;

        private Rigidbody rb;
        private AlienCooldown cooldown;
        private GameObject target;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            cooldown = GetComponent<AlienCooldown>();
        }

        void Start()
        {
            RegisterIfReady();
        }

        void FixedUpdate()
        {
            switch (state)
            {
                case AlienWaitState.Attacking:
                    if (target != null)
                        MoveTowards(target.transform.position);
                    break;

                case AlienWaitState.Returning:
                    MoveTowards(
                        AlienWaitCoordinator.Instance.mothership.position
                    );

                    // Rejoin group once cooldown finishes
                    if (cooldown != null && !cooldown.IsCoolingDown)
                    {
                        state = AlienWaitState.Waiting;
                        RegisterIfReady();
                    }
                    break;
            }
        }

        void MoveTowards(Vector3 destination)
        {
            Vector3 direction = (destination - transform.position).normalized;
            rb.MovePosition(
                rb.position + direction * moveSpeed * Time.fixedDeltaTime
            );
        }

        void RegisterIfReady()
        {
            if (cooldown != null && cooldown.IsCoolingDown) return;

            AlienWaitCoordinator.Instance.RegisterAlien(this);
        }

        public void StartAttack(GameObject attackTarget)
        {
            if (cooldown != null && cooldown.IsCoolingDown) return;

            target = attackTarget;
            state = AlienWaitState.Attacking;
        }

        void OnTriggerEnter(Collider other)
        {
            if (state != AlienWaitState.Attacking) return;

            if (other.CompareTag("Civilian") || other.CompareTag("Player"))
            {
                BeginReturn();
            }
        }

        void BeginReturn()
        {
            state = AlienWaitState.Returning;

            if (cooldown != null)
                cooldown.StartCooldown();

            AlienWaitCoordinator.Instance.ResetAttack();
        }
    }
}
