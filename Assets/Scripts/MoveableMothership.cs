using System.Collections;
using UnityEngine;

public class MoveableMothership : MothershipBase
{
    [SerializeField] protected float moveSpeed;

    [SerializeField] protected GameObject[] waypoints;

    [SerializeField] private AudioClip[] engineSounds;

    protected override void Update()
    {
        base.Update();

        if (isSpawningAliens == false)
        {
            MoveToAWaypoint();
        }
    }

    protected override void SpawnAliens()
    {
        base.SpawnAliens();
    }

    protected override IEnumerator SpawnTimer()
    {
        yield return StartCoroutine(MoveToAWaypoint());

        yield return StartCoroutine(base.SpawnTimer());
    }

    protected IEnumerator MoveToAWaypoint()
    {
        int randomIndex = Random.Range(0, waypoints.Length - 1);
        Debug.Log(randomIndex);
        PlayRandomEngineSound();
        while ((waypoints[randomIndex].transform.position - transform.position).magnitude > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, waypoints[randomIndex].transform.position, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = waypoints[randomIndex].transform.position;
        audioSource.Stop();
    }

    private void PlayRandomEngineSound()
    {
        int randomIndex = Random.Range(0, engineSounds.Length - 1);
        audioSource.clip = engineSounds[randomIndex];
        audioSource.Play();
    }

}
