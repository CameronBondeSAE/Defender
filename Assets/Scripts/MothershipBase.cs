using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class MothershipBase : MonoBehaviour
{
    [SerializeField] protected GameObject alienPrefab;
    [SerializeField] protected GameObject blueBeam;
    
    [SerializeField] protected GameObject[] waypoints;
    
    [SerializeField] protected int alienSpawnCount; //number of aliens that spawn at a time
    [SerializeField] protected float spawnDelay; //the time in seconds it takes to spawn aliens again

    [SerializeField] protected float blueBeamDuration;
    //[SerializeField] protected float spawnTimer;
    [SerializeField] protected bool isSpawningAliens;

    [SerializeField] protected float moveSpeed;

    [SerializeField] protected float raycastLength;
    [SerializeField] protected LayerMask raycastPhysicsLayerMasks;

    private Vector3 alienSpawnPosition; //The position where the alien will spawn on the map
    
    void Start()
    {
        //StartCoroutine(SpawnTimer());
        isSpawningAliens = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnAliens();
        }

        if (isSpawningAliens == false)
        {
            MoveToAWaypoint();
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
        isSpawningAliens = true;
        yield return StartCoroutine(MoveToAWaypoint());
        
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
            Instantiate(alienPrefab, alienSpawnPosition + new Vector3(0,1,0), Quaternion.identity);
            yield return new WaitForSeconds(blueBeamDuration);
            blueBeam.SetActive(false);
            yield return new WaitForSeconds(spawnDelay);
            //MoveToAWaypoint();
        }
        isSpawningAliens = false;
    }

    protected virtual IEnumerator MoveToAWaypoint()
    {
        int randomIndex = Random.Range(0, waypoints.Length - 1);
        Debug.Log(randomIndex);
        while ((waypoints[randomIndex].transform.position - transform.position).magnitude > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, waypoints[randomIndex].transform.position, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = waypoints[randomIndex].transform.position;
        
    }
    
}
