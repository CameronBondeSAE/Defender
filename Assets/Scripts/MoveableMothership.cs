using System.Collections;
using UnityEngine;


namespace mothershipScripts
{
    /// <summary>
    /// this mothership will move between set waypoints and spawn aliens there.
    /// </summary>
    public class MoveableMothership : MothershipBase, IMovable, ISpawner
    {
        //[SerializeField] protected float moveSpeed;

        [SerializeField] private GameObject[] waypoints;

        //[SerializeField] protected AudioClip[] engineSounds;

        [SerializeField] protected Movement movement;

        protected override void Update()
        {
            base.Update();

            //if (isSpawningAliens == false)
            //{
            //    MoveToAWaypoint();
            //}
        }

        protected override IEnumerator SpawnTimer()
        {
            int randomIndex = Random.Range(0, waypoints.Length - 1);
            if (waypoints[randomIndex] != null)
            {
                yield return StartCoroutine(MoveToPosition(waypoints[randomIndex].transform.position));
            }

            yield return StartCoroutine(base.SpawnTimer());
        }

        public IEnumerator MoveToPosition(Vector3 targetPosition)
        {
            yield return StartCoroutine(movement.MoveToADestination(targetPosition));
        }

        //private IEnumerator MoveToAWaypoint()
        //{
        //    Debug.Log("test");
        //    int randomIndex = Random.Range(0, waypoints.Length - 1);
        //    Debug.Log(randomIndex);
        //    PlayRandomEngineSound();
        //    while ((waypoints[randomIndex].transform.position - transform.position).magnitude > 0.01f)
        //    {
        //        transform.position = Vector3.Lerp(transform.position, waypoints[randomIndex].transform.position, moveSpeed * Time.deltaTime);
        //        yield return null;
        //    }
        //    transform.position = waypoints[randomIndex].transform.position; //Snap position to waypoint position
        //    audioSource.Stop();
        //}

        //protected void PlayRandomEngineSound()
        //{
        //    int randomIndex = Random.Range(0, engineSounds.Length - 1);
        //    audioSource.clip = engineSounds[randomIndex];
        //    audioSource.Play();
        //}

    }
}