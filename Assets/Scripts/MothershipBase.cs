using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace mothershipScripts
{
    /// <summary>
    /// This motherhship doesnt move at all, it jsut spawns aliens below it
    /// </summary>

    public class MothershipBase : NetworkBehaviour, ISpawner
    {
        [SerializeField] protected GameObject alienPrefab;
        [SerializeField] protected GameObject blueBeam;

        [SerializeField] protected int alienSpawnCountForEachWave; //number of aliens that spawn at a time
        [SerializeField] protected float spawnDelay; //the time in seconds it takes to spawn aliens again
        [SerializeField] protected float spawnDelayBetweenWaves = 5f; //the time in seconds it takes to spawn aliens again
        [SerializeField] protected int maxWaves;
        [SerializeField] protected float currentWaveNumber;

        [SerializeField] protected float blueBeamDuration;

        //[SerializeField] protected float spawnTimer;
        [SerializeField] protected bool isSpawningAliens;

        [SerializeField] protected float raycastLength;
        [SerializeField] protected LayerMask raycastPhysicsLayerMasks;

        private Vector3 alienSpawnPosition; //The position where the alien will spawn on the map
        [SerializeField] protected Vector3 alienSpawnOffset;

        [SerializeField] protected float rotationSpeed;

        //audio stuff
        [SerializeField] protected AudioClip[] beamSounds;
        
        public delegate void AlienSpawned(GameObject alien);
        public event AlienSpawned OnAlienSpawned;
        
        protected AudioSource audioSource;
        public    int         alienSpawnCount;

        public event Action<GameObject> AlienSpawned_Event;

        private void OnEnable()
        {
	        DanniLi.GameManager gameManager = FindFirstObjectByType<DanniLi.GameManager>();
	        gameManager.RegisterWaveSpawner(this);
        }

        private void OnDisable()
        {
	        DanniLi.GameManager gameManager = FindFirstObjectByType<DanniLi.GameManager>();
	        gameManager.DeregisterWaveSpawner(this);
        }

        protected virtual void Start()
        {
            //StartCoroutine(SpawnTimer());
            isSpawningAliens = false;
            audioSource = transform.GetComponent<AudioSource>();
            
            // FindAnyObjectByType<GameManager>().
        }

        protected virtual void Update()
        {
            Spin();

            // if (InputSystem.GetDevice<Keyboard>().spaceKey.wasPressedThisFrame)
            // {
            //     //SpawnAliens();
            //     StartWaves();
            // }
        }

        protected virtual IEnumerator SpawnAliens()
        {
            for (int i = 0; i < maxWaves; i++)
            {
                if (isSpawningAliens == false)
                {
                    yield return StartCoroutine(SpawnTimer());
                    yield return new WaitForSeconds(spawnDelayBetweenWaves);
                }
            }
            Debug.Log("Waves finished");
            blueBeam.SetActive(true);
            blueBeam.transform.GetComponent<Collider>().enabled = true;
            blueBeam.transform.GetComponent<Collider>().isTrigger = true;
        }

        protected virtual IEnumerator SpawnTimer()
        {
            Debug.Log("test");
            isSpawningAliens = true;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hit, raycastLength,
                    raycastPhysicsLayerMasks))
            {
                alienSpawnPosition = hit.point;
            }

            Debug.Log("wave of aliens spawning");
            for (int i = 0; i < alienSpawnCountForEachWave; i++)
            {
                blueBeam.SetActive(true);
                PlayRandomBeamSound();
                GameObject alienSpawned = Instantiate(alienPrefab, alienSpawnPosition + alienSpawnOffset, Quaternion.identity);
                // TODO: Make abstract
                alienSpawned.GetComponent<AlienAI>().mothership = transform;

                OnAlienSpawned?.Invoke(alienSpawned);
                yield return new WaitForSeconds(blueBeamDuration);
                blueBeam.SetActive(false);
                yield return new WaitForSeconds(spawnDelay);
                //MoveToAWaypoint();
            }

            isSpawningAliens = false;
        }

        /// <summary>
        /// this function can be used by a GameManager script to activate waves
        /// </summary>
        public void StartWaves()
        { 
            StartCoroutine(SpawnAliens());
        }

        protected void Spin()
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }

        protected void PlayRandomBeamSound()
        {
            int randomIndex = Random.Range(0, beamSounds.Length - 1);
            audioSource.clip = beamSounds[randomIndex];
            audioSource.Play();
        }

    }
}
