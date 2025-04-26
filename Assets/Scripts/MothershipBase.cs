using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class MothershipBase : MonoBehaviour
{
    [SerializeField] protected GameObject alienPrefab;
    [SerializeField] protected GameObject blueBeam;
    
    [SerializeField] protected int alienSpawnCount; //number of aliens that spawn at a time
    [SerializeField] protected float spawnDelay; //the time in seconds it takes to spawn aliens again

    [SerializeField] protected float blueBeamDuration;
    //[SerializeField] protected float spawnTimer;
    [SerializeField] protected bool isSpawningAliens;

    [SerializeField] protected float raycastLength;
    [SerializeField] protected LayerMask raycastPhysicsLayerMasks;

    private Vector3 alienSpawnPosition; //The position where the alien will spawn on the map
    [SerializeField] protected Vector3 alienSpawnOffset;
    
    protected virtual void Start()
    {
        //StartCoroutine(SpawnTimer());
        isSpawningAliens = false;
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnAliens();
        }
    }

    protected virtual void SpawnAliens()
    {
        if (isSpawningAliens == false)
        {
            StartCoroutine(SpawnTimer());
        }
        
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
            Instantiate(alienPrefab, alienSpawnPosition + alienSpawnOffset, Quaternion.identity);
            yield return new WaitForSeconds(blueBeamDuration);
            blueBeam.SetActive(false);
            yield return new WaitForSeconds(spawnDelay);
            //MoveToAWaypoint();
        }
        isSpawningAliens = false;
    }
    
    /// <summary>
    /// function to be used by GameManager script to activate waves.
    /// </summary>
    public void StartWave()
    {
        SpawnAliens();
    }

}
