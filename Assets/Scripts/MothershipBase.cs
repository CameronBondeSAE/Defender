using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class MothershipBase : MonoBehaviour
{
    [SerializeField] protected GameObject alienPrefab;

    [SerializeField] protected GameObject[] waypoints;
    
    [SerializeField] protected int alienSpawnCount; //number of aliens that spawn at a time
    [SerializeField] protected int spawnDelay; //the time in seconds it takes to spawn aliens again
    //[SerializeField] protected float spawnTimer;

    [SerializeField] private float raycastLength;
    [SerializeField] private LayerMask raycastPhysicsLayerMasks;

    [SerializeField] private Vector3 alienSpawnPosition; //The position where the alien will spawn on the map
    
    void Start()
    {
        //StartCoroutine(SpawnTimer());

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnAliens(alienSpawnCount);
        }
    }

    protected void SpawnAliens(int numberOfAliens)
    {
        
        Debug.Log("wave of aliens spawning");

        
        StartCoroutine(SpawnTimer());
        
    }

    IEnumerator SpawnTimer()
    {
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, new Vector3(0, -1, 0), out hit, raycastLength,
                raycastPhysicsLayerMasks))
        {
            alienSpawnPosition = hit.point;
        }
        
        for (int i = 0; i < alienSpawnCount; i++)
        {
            Instantiate(alienPrefab, alienSpawnPosition , Quaternion.identity);
            yield return new WaitForSeconds(spawnDelay);
        }
    }
    
}
