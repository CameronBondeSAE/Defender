using System.Collections;
using UnityEngine;
/// <summary>
/// This mothership will constantly move towards the players position and spawn aliens near them
/// </summary>

public class AggressiveMothership : MothershipBase
{
    [SerializeField] private float detectionSphereRadius;

    [SerializeField] private float yPosition;

    [SerializeField] protected Movement movement;

    protected override void Start()
    {
        base.Start();
        yPosition = transform.position.y;
    }

    protected override IEnumerator SpawnTimer()
    {
        yield return StartCoroutine(CheckForPlayer());
        yield return StartCoroutine(base.SpawnTimer());

    }

    protected override void SpawnAliens()
    {
        base.SpawnAliens();
    }

    private IEnumerator CheckForPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionSphereRadius);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].transform.tag == "Player")
            {
                Vector3 playerPositionWithYOffset = new Vector3(hits[i].transform.position.x, yPosition, hits[i].transform.position.z);
                yield return StartCoroutine(movement.MoveToADestination(playerPositionWithYOffset));
            }
        }
       // Debug.Log("test");
    }

    //private IEnumerator MoveTowardsPlayer(Vector3 currentPlayerPosition)
    //{
    //    PlayRandomEngineSound();
    //    Vector3 destination = new Vector3(currentPlayerPosition.x, yPosition, currentPlayerPosition.z);
    //    while ((destination - transform.position).magnitude > 0.01f)
    //    {
    //        transform.position = Vector3.Lerp(transform.position, destination, moveSpeed * Time.deltaTime);
    //        yield return null;
    //    }
    //    transform.position = destination;
    //    audioSource.Stop();

    //}

    /// <summary>
    /// shows detection area
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionSphereRadius);
    }

}
