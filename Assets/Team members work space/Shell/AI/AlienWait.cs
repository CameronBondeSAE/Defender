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
        public float moveSpeed = 3f;
        public AlienState state = AlienState.Waiting;

        private Rigidbody rb;
        private AlienCooldown cooldown;
        private GameObject target;

        private bool hasRegistered = false;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            cooldown = GetComponent<AlienCooldown>();
        }

        void Start()
        {
            Debug.Log("AlienWait STARTED on " + gameObject.name);
            TryRegister();
        }

        void FixedUpdate()
        {
            switch (state)
            {
                case AlienState.Attacking:
                    if (target != null)
                        MoveTowards(target.transform.position);
                    break;

                case AlienState.Returning:
                    MoveTowards(
                        AlienWaitCoordinator.Instance.mothership.position
                    );

                    if (cooldown != null && !cooldown.IsCoolingDown)
                    {
                        state = AlienState.Waiting;
                        TryRegister();
                    }
                    break;
            }
        }

        void MoveTowards(Vector3 destination)
        {
            Vector3 dir = (destination - transform.position).normalized;
            rb.MovePosition(
                rb.position + dir * moveSpeed * Time.fixedDeltaTime
            );
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
            Debug.Log("Alien attacking target");
        }

        void OnTriggerEnter(Collider other)
        {
            if (state != AlienState.Attacking) return;

            if (other.CompareTag("Civilian") ||
                other.CompareTag("Player"))
            {
                Debug.Log("Alien hit target → returning");
                BeginReturn();
            }
        }

        void BeginReturn()
        {
            state = AlienState.Returning;
            cooldown?.StartCooldown();
            AlienWaitCoordinator.Instance.ResetCoordinator();
            hasRegistered = false;
        }
    }
}
