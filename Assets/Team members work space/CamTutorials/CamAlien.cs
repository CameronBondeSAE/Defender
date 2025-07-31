using UnityEngine;
using UnityEngine.AI;

public class CamAlien : MonoBehaviour
{
	public float time = 5f;
	

	public float rng;
	public CamSpawn[] camSpawns;
	NavMeshAgent navMeshAgent;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		rng = Random.Range(4f, 13f);

		camSpawns = FindObjectsByType<CamSpawn>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

		navMeshAgent = GetComponent<NavMeshAgent>();

		FindCiv();
	}

	// Update is called once per frame
	void Update()
	{
		// time += Time.deltaTime;
		// if (time > rng)
		// {
		// time = 0;

		if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + 0.5f)
		{
			FindCiv();
		}
		// }
	}

	private void FindCiv()
	{
		CamSpawn camSpawn = camSpawns[Random.Range(0, camSpawns.Length)];
		navMeshAgent.SetDestination(camSpawn.transform.position);
	}
}