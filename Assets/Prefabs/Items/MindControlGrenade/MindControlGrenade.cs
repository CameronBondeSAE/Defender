using System.Collections.Generic;
using UnityEngine;


    /// <summary>
    /// script for the mind control grenade
    /// </summary>
    public class MindControlGrenade : MonoBehaviour , IUsable, IPickup
    {
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private float countDownTime = 5f;
        
        [Header("Visuals")]
        [SerializeField] private List<MeshRenderer> ringMeshRenderers;
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material inactiveMaterial;
        
        private bool isActivated = false;
        private float countDown;

        #region UnityFunctions
        void Start()
        {
            countDown = countDownTime;
        }

        void Update()
        {
            if (isActivated)
            {
                countDown -= Time.deltaTime;
                if (countDown <= 0)
                {
                    Explode();
                }
            }
        }
        #endregion
        
        /// <summary>
        /// Activate grenade
        /// </summary>
        public void Use()
        {
            isActivated = true; // starts 5 second timer, after 5 seconds will EXPLODE
            foreach (MeshRenderer meshRenderer in ringMeshRenderers)
            {
                meshRenderer.material = activeMaterial;
            }
        }

        /// <summary>
        /// Deactivate grenade
        /// </summary>
        public void StopUsing()
        {
            isActivated = false;
            foreach (MeshRenderer meshRenderer in ringMeshRenderers)
            {
                meshRenderer.material = inactiveMaterial;
            }
            countDown = countDownTime; // resets timer so when activated it doesn't blow up too quick
        }
        
        public void Pickup()
        {
            // make sound
        }

        // want drop to throw
        public void Drop()
        {
            Use(); // throw should also activate
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
                    if (character.GetComponent<AIBase>() != null)
                    {
                        // change the AI to something
                        Debug.Log(character.name + " has been hit by mind control grenade");
                    }
                }
            }
            
            Destroy(gameObject);
        }
        
    }

