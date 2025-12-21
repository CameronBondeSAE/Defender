using UnityEngine;

namespace Shell_AI
{
    public enum AlienState
    {
        Waiting,
        Attacking,
        Returning
    }

    public class AlienWait : MonoBehaviour
    {
        public float moveSpeed = 6f;
        public float stopDistance = 0.8f;

        public AlienState state = AlienState.Waiting;

        Rigidbody rb;
        AlienCooldown cooldown;
        GameObject target;

        bool hasRegistered;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            cooldown = GetComponent<AlienCooldown>();
        }

        void Start()
        {
            TryRegister();
        }

        void FixedUpdate()
        {
            if (state == AlienState.Attacking && target != null)
            {
                MoveTo(target.transform.position);
                return;
            }

            if (state == AlienState.Returning)
            {
                MoveTo(AlienWaitCoordinator.Instance.mothership.position);

                if (Vector3.Distance(
                        transform.position,
                        AlienWaitCoordinator.Instance.mothership.position
                    ) < stopDistance)
                {
                    FinishReturn();
                }

                return;
            }

            // Waiting state → DO NOTHING
        }

        void MoveTo(Vector3 destination)
        {
            Vector3 flatDest = new Vector3(
                destination.x,
                transform.position.y,
                destination.z
            );

            float dist = Vector3.Distance(transform.position, flatDest);

            if (dist < stopDistance)
            {
                rb.linearVelocity = Vector3.zero;
                return;
            }

            Vector3 dir = (flatDest - transform.position).normalized;

            rb.linearVelocity = dir * moveSpeed;
        }

        void TryRegister()
        {
            if (hasRegistered) return;
            if (cooldown != null && cooldown.IsCoolingDown) return;

            hasRegistered = true;
            AlienWaitCoordinator.Instance.RegisterAlien(this);
        }

        public void StartAttack(GameObject attackTarget)
        {
            hasRegistered = false;
            target = attackTarget;
            state = AlienState.Attacking;
        }

        void OnTriggerEnter(Collider other)
        {
            if (state != AlienState.Attacking) return;

            if (other.CompareTag("Civilian") || other.CompareTag("Player"))
            {
                BeginReturn();
            }
        }

        void BeginReturn()
        {
            state = AlienState.Returning;
            rb.linearVelocity = Vector3.zero;
            cooldown?.StartCooldown();
            AlienWaitCoordinator.Instance.ResetCoordinator();
        }

        void FinishReturn()
        {
            rb.linearVelocity = Vector3.zero;
            state = AlienState.Waiting;
            hasRegistered = false;
            TryRegister();
        }
    }
}
