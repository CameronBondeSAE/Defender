using UnityEngine;


    /// <summary>
    /// script for the mind control grenade
    /// </summary>
    public class MindControlGrenade : MonoBehaviour , IUsable, IPickup
    {
        [SerializeField] private float explosionRadius;
        [SerializeField] private float countDownTimer = 5f;
        
        private bool isActivated = false;
        private float countDown;

        void Start()
        {
            countDown = countDownTimer;
        }

        void Update()
        {
            if (isActivated)
            {
                countDownTimer -= Time.deltaTime;
                if (countDown <= 0)
                {
                    Explode();
                }
            }
        }
        
        
        /// <summary>
        /// Activate grenade
        /// </summary>
        public void Use()
        {
            isActivated = true;
            Debug.Log("MindControlGrenade activated");
            // starts 5 second timer, after 5 seconds EXPLODE
        }

        /// <summary>
        /// Deactivate grenade
        /// </summary>
        public void StopUsing()
        {
            isActivated = false;
            countDown = countDownTimer; // resets timer so when activated it doesnt blow up too quick
        }
        
        public void Pickup()
        {
            // make sound
        }

        // want drop to be throw
        public void Drop()
        {
            
        }
        private void Explode()
        {
            // play sound
            // particle effects
            
            // check for radius around grenade
            Collider[] charactersHit = Physics.OverlapSphere(transform.position, explosionRadius);
            // change AI of all character bases in radius - ?after x amount of time, revert to previous AI state?
            if (charactersHit.Length > 0)
            {
                foreach (Collider character in charactersHit)
                {
                    // if(character.GetComponent<>()) what common component do aliens & civilians have in common that I can check for?
                }
            }
        }
        
        
        
        
        // after launched will need to explode, have explosion radius, change ai of aliens hit - maybe even civilians?, play sound, spawn particles
    }

