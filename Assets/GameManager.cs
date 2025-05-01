using System;
using System.Collections.Generic;
using System.Linq;
using mothershipScripts;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

public class GameManager : MonoBehaviour
{
	public GameObject winScreen;
	public GameObject loseScreen;
	
	public Object[] levels;
	public int currentLevelIndex;
	
	public PlayerInputManager playerInputManager;
	public List<GameObject> players;
	public CinemachineTargetGroup targetGroup;

	public event Action GetReady_Event;
	public event Action StartGame_Event;
	public event Action WinGameOver_Event;
	public event Action LoseGameOver_Event;

	public int civiliansAlive;
	public int aliensIncoming;
	public LevelInfo levelInfo;

	public GameObject[] civilians;
	public MothershipBase[] mothershipBases;

	private void OnEnable()
	{
		playerInputManager.onPlayerJoined += OnPlayerJoin;
		playerInputManager.onPlayerLeft += OnPlayerLeave;

		mothershipBases = FindObjectsByType<MothershipBase>(FindObjectsInactive.Include, FindObjectsSortMode.None);

		aliensIncoming = 0;
		foreach (MothershipBase mothershipBase in mothershipBases)
		{
			aliensIncoming += mothershipBase.alienSpawnCount;
			mothershipBase.AlienSpawned_Event += MothershipBaseOnAlienSpawned_Event;
		}

		Debug.Log("Aliens Incoming: " + aliensIncoming);

		civilians = GameObject.FindGameObjectsWithTag("Civilian");
		civiliansAlive = civilians.Length;
		Debug.Log("Civilians Alive: " + civiliansAlive);

		foreach (GameObject civilian in civilians)
		{
			civilian.GetComponent<Health>().OnDeath += OnCivDeath;
		}

		levelInfo = FindFirstObjectByType<LevelInfo>();
		Debug.Log("Level Info: Civilian percentage to save" + levelInfo.percentageToSave);
	}

	private void MothershipBaseOnAlienSpawned_Event(GameObject obj)
	{
		obj.GetComponent<Health>().OnDeath += OnAlienDeath;
	}

	private void OnAlienDeath()
	{
		GameObject[] aliens = GameObject.FindGameObjectsWithTag("Alien");

		if (aliens.Length <= 0)
		{
			Debug.Log("Game Over: Win");
			WinGameOver_Event?.Invoke();
			loseScreen.SetActive(false);
			winScreen.SetActive(true);
		}
	}

	private void OnCivDeath()
	{
		civiliansAlive--;

		// Game over. Too many civs dead
		if (civiliansAlive < civilians.Length * (levelInfo.percentageToSave / 100f))
		{
			Debug.Log("Game Over: Loss");
			LoseGameOver_Event?.Invoke();
			winScreen.SetActive(false);
			loseScreen.SetActive(true);
		}
	}

	// Add player to target group when they join
	public void OnPlayerJoin(PlayerInput playerInput)
	{
		AddPlayerToTargetGroup(playerInput.transform);
	}

	// Remove player from target group when they leave
	public void OnPlayerLeave(PlayerInput playerInput)
	{
		RemovePlayerFromTargetGroup(playerInput.transform);
	}

	private void AddPlayerToTargetGroup(Transform playerTransform)
	{
		if (targetGroup != null)
		{
			CinemachineTargetGroup.Target newTarget = new CinemachineTargetGroup.Target();
			newTarget.Object = playerTransform;
			newTarget.Weight = 1f; // Optional: Customize weight if needed
			newTarget.Radius = 1f; // Optional: Customize radius if needed

			List<CinemachineTargetGroup.Target> targets = targetGroup.m_Targets.ToList();
			targets.Add(newTarget);
			targetGroup.m_Targets = targets.ToArray();
		}
	}

	private void RemovePlayerFromTargetGroup(Transform playerTransform)
	{
		if (targetGroup != null)
		{
			List<CinemachineTargetGroup.Target> targets = targetGroup.m_Targets.ToList();
			foreach (CinemachineTargetGroup.Target target in targets)
			{
				if (target.Object == playerTransform)
					targets.Remove(target);
			}

			targetGroup.m_Targets = targets.ToArray();
		}
	}
}