using UnityEngine;
using UnityEngine.AI;

public class CamCivilian : MonoBehaviour
{
	public float followDistance = 3f;
	public CamAlien[] camAliens;
	NavMeshAgent navMeshAgent;
	CamAlien targetCamAlien;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		camAliens = FindObjectsByType<CamAlien>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		targetCamAlien = camAliens[Random.Range(0, camAliens.Length)];

		
		navMeshAgent = GetComponent<NavMeshAgent>();
		navMeshAgent.SetDestination(targetCamAlien.transform.position);
	}

	// Update is called once per frame
	void Update()
	{
		if (Vector3.Distance(transform.position, targetCamAlien.transform.position) > followDistance)
		{
			navMeshAgent.SetDestination(targetCamAlien.transform.position);
			navMeshAgent.isStopped = false;
		}
		else
		{
			navMeshAgent.isStopped = true;
		}
	}
}
