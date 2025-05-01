using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace mothershipScripts
{
    /// <summary>
    /// This motherhship doesnt move at all, it jsut spawns aliens below it
    /// </summary>

    public class MothershipBase : MonoBehaviour
    {
        [SerializeField] protected GameObject alienPrefab;
        [SerializeField] protected GameObject blueBeam;

        [SerializeField]
        public int alienSpawnCount; //number of aliens that spawn at a time
        [SerializeField] protected float spawnDelay; //the time in seconds it takes to spawn aliens again
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
        protected AudioSource audioSource;

        public event Action<GameObject> AlienSpawned_Event;

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

            // if (Input.GetKeyDown(KeyCode.E))
            // {
            //     //SpawnAliens();
            //     StartWaves(maxWaves);
            // }
        }

        protected virtual IEnumerator SpawnAliens(int numberOfWaves)
        {
            for (int i = 0; i < numberOfWaves; i++)
            {
                if (isSpawningAliens == false)
                {
                    yield return StartCoroutine(SpawnTimer());
                    yield return new WaitForSeconds(spawnDelay);
                }
            }
            Debug.Log("Waves finished");
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
            for (int i = 0; i < alienSpawnCount; i++)
            {
                blueBeam.SetActive(true);
                PlayRandomBeamSound();
                GameObject newAlien = Instantiate(alienPrefab, alienSpawnPosition + alienSpawnOffset, Quaternion.identity);
                // Cam added
                AlienSpawned_Event?.Invoke(newAlien);
                
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
        public void StartWaves(int numberOfWaves)
        { 
            StartCoroutine(SpawnAliens(numberOfWaves));
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
