using System;
using System.Collections.Generic;
using System.Linq;
using CameronBonde;
using mothershipScripts;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DanniLi
{
	public class GameManager : NetworkBehaviour
	{
		public GameObject winScreen;
		public GameObject loseScreen;

		public int currentLevelIndex;

		[Header("References")]
		[SerializeField]
		public LevelInfo[] levelSOs;

		public CinemachineTargetGroup targetGroup;

		private PlayerInventory playerInventory;

		[Header("Spawn Settings")]
		[SerializeField]
		private Transform levelContainer; // for organization

		[Header("Debugging: Don't edit")]
		private LevelInfo currentlevelInfo;

		public List<ISpawner> spawners = new List<ISpawner>();

		public int totalCivilians;
		public int civiliansAlive;
		public int aliensIncomingFromAllShips;

		public GameObject[]     civilians;
		public MothershipBase[] mothershipBases;


		public event Action GetReady_Event;
		public event Action StartGame_Event;
		public event Action WinGameOver_Event;
		public event Action LoseGameOver_Event;

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();

			if (IsServer)
			{
				GetReady_Event?.Invoke();
			}
		}

		private void Start()
		{
			// FindPlayerInventory();
			// SetupPlayerInventoryItems();
			// SpawnLevelProps();

			StartWave();
		}

		private void OnEnable()
		{
			// playerInputManager.onPlayerJoined += OnPlayerJoin;
			// playerInputManager.onPlayerLeft   += OnPlayerLeave;
			// if(IsClient) // Camera is client side only for now
			NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerJoin;
			NetworkManager.Singleton.OnConnectionEvent         += SingletonOnOnConnectionEvent;

			mothershipBases = FindObjectsByType<MothershipBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

			aliensIncomingFromAllShips = 0;
			foreach (MothershipBase mothershipBase in mothershipBases)
			{
				aliensIncomingFromAllShips        += mothershipBase.totalAlienSpawnCount;
				mothershipBase.AlienSpawned_Event += MothershipBaseOnAlienSpawned_Event;
			}

			// Set all crates to know this level's set of item SOs
			Crate[] crates = FindObjectsByType<Crate>(FindObjectsSortMode.None);
			foreach (Crate crate in crates)
			{
				LevelInfo levelSO                         = levelSOs[currentLevelIndex];
				if (levelSO != null) crate.availableItems = levelSO.availableItems;
			}

			Debug.Log("Aliens Incoming: " + aliensIncomingFromAllShips);

			civilians      = GameObject.FindGameObjectsWithTag("Civilian");
			totalCivilians = civilians.Length;
			civiliansAlive = civilians.Length;
			Debug.Log("Civilians Alive: " + civiliansAlive);

			foreach (GameObject civilian in civilians)
			{
				Health health = civilian.GetComponent<Health>();
				if (health != null)
				{
					health.OnDeath += OnCivDeath;
				}
			}

			// TODO: Probably keep levelinfo a SO file, as we need the ability to show level names/info BEFORE the level is loaded
			// levelInfo = FindFirstObjectByType<LevelInfo>();
			if (currentlevelInfo != null)
			{
				Debug.Log("Level Info: Civilians to save" + currentlevelInfo.civiliansToSave);
			}
			else
			{
				Debug.Log("Level Info: No level info found");
			}
		}

		private void SingletonOnOnConnectionEvent(NetworkManager arg1, ConnectionEventData arg2)
		{
			if (arg2.EventType == ConnectionEvent.ClientConnected)
			{
				Debug.Log("Client connected : " + arg2.ClientId);
			}
		}

		private void OnDisable()
		{
			if (IsClient) // Camera is client side only for now
				NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerJoin;
		}


		private void MothershipBaseOnAlienSpawned_Event(GameObject obj)
		{
			Health health = obj.GetComponent<Health>();
			if (health != null)
			{
				health.OnDeath += OnAlienDeath;
			}
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
			// if (civiliansAlive < civilians.Length * (currentlevelInfo.percentageToSave / 100f))
			if (civiliansAlive < civilians.Length - civiliansAlive)
			{
				Debug.Log("Game Over: Loss");
				LoseGameOver_Event?.Invoke();
				winScreen.SetActive(false);
				loseScreen.SetActive(true);
			}
		}


		// Add player to target group when they join
		public void OnPlayerJoin(ulong playerID)
		{
			Debug.Log("Player joined called " + playerID);
			if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerID, out NetworkClient client))
			{
				Debug.Log("Player joined: " + client.PlayerObject.name);
				if (client.PlayerObject.IsLocalPlayer)
				{
					AddItemToCameraTargetGroup(client.PlayerObject.transform);
				}
			}
		}

		// Remove player from target group when they leave
		public void OnPlayerLeave(ulong playerID)
		{
			if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerID, out NetworkClient client))
			{
				Debug.Log("Player left: " + client.PlayerObject.name);

				if (client.PlayerObject.IsLocalPlayer)
				{
					RemoveItemFromCameraTargetGroup(client.PlayerObject.transform);
				}
			}
		}

		private void AddItemToCameraTargetGroup(Transform playerTransform)
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

		private void RemoveItemFromCameraTargetGroup(Transform playerTransform)
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


		public void TestLoadScene()
		{
			SceneHelper.LoadScene("Main Menu", true, true);
		}

		public void TestUnloadScene()
		{
			SceneHelper.UnloadScene("Main Menu");
		}

		private void StartWave()
		{
			foreach (ISpawner spawner in spawners)
			{
				spawner.StartWaves();
			}
		}

		public void RegisterWaveSpawner(ISpawner spawner)
		{
			spawners.Add(spawner);
		}

		public void DeregisterWaveSpawner(ISpawner spawner)
		{
			spawners.Remove(spawner);
		}

		// private void FindPlayerInventory()
		// {
		// 	GameObject player = GameObject.FindGameObjectWithTag("Player");
		//
		// 	if (player == null)
		// 	{
		// 		return;
		// 	}
		//
		// 	playerInventory = player.GetComponent<PlayerInventory>();
		// 	if (playerInventory == null)
		// 	{
		// 		Debug.LogError("GameManager: please attach PlayerInventory to player!");
		// 	}
		// }

		// private void SetupPlayerInventoryItems()
		// {
		// 	if (playerInventory == null)
		// 	{
		// 		return;
		// 	}
		//
		// 	if (currentlevelInfo == null)
		// 	{
		// 		return;
		// 	}
		//
		// 	foreach (var item in currentlevelInfo.availableItems)
		// 	{
		// 		if (item != null)
		// 		{
		// 			playerInventory.RegisterAvailableItem(item);
		// 			Debug.Log($"GameManager: Registered item {item.name}");
		// 		}
		// 		else
		// 		{
		// 			Debug.LogWarning("GameManager: please assign items in LevelInfo!");
		// 		}
		// 	}
		// }

		private void SpawnLevelProps()
		{
			if (currentlevelInfo == null)
			{
				Debug.LogWarning("GameManager: no level info!");
				return;
			}

			for (int i = 0; i < currentlevelInfo.wallSpawnCount; i++)
			{
				Instantiate(currentlevelInfo.wallPrefab, GetRandomSpawnPosition(), Quaternion.identity, levelContainer);
			}

			for (int i = 0; i < currentlevelInfo.crateSpawnCount; i++)
			{
				if (currentlevelInfo.cratePrefab != null)
					Instantiate(currentlevelInfo.cratePrefab, GetRandomSpawnPosition(), Quaternion.identity,
					            levelContainer);
				else
				{
					Debug.LogWarning("GameManager: no crate prefab!");
				}
			}
		}

		private Vector3 GetRandomSpawnPosition()
		{
			return new Vector3(Random.Range(-50, 50), -0.5f, Random.Range(-50, 50)); // adjust these based on level size
		}
	}
}