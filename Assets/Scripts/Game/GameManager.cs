using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CameronBonde;
using mothershipScripts;
using NUnit.Framework.Internal.Commands;
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
		[Header("Level Information")]
		[SerializeField]
		public LevelInfo[] levelSOs;
		[Header("Debugging: Don't edit")]
		private LevelInfo currentlevelInfo;
		public int currentLevelIndex = 0;
		[SerializeField]
		private Transform levelContainer; // for organization

		[Header("Camera Set-Up")]
		public CinemachineTargetGroup targetGroup;

		[Header("References")]
		private PlayerInventory playerInventory;
		[SerializeField] private UIManager uiManager;

		[Header("Spawner Settings")]
		public List<ISpawner> spawners = new List<ISpawner>();
		
		[Header("Alien Params")]
		public int aliensIncomingFromAllShips;
		private int totalAliensPlanned;
		private int aliensSpawnedSoFar;
		private int aliensDeadSoFar;
		
		[Header("Civilian Params")]
		public int totalCivilians;
		public int civiliansAlive;
		public GameObject[]     civilians;
		
		[Header("Mothership Params")]
		public MothershipBase[] mothershipBases;

		[Header("Difficulty Settings")] 
		private float enemyMult = 1f; 
		private float waveMult = 1f;
		
		[Header("Wave Management")]
		private int currentWaveNumber = 0;
		private int totalWaves = 3;
		private bool waveInProgress;
		private Coroutine startFlowCoroutine;

		// Events
		public event Action GetReady_Event;
		public event Action StartGame_Event;
		public event Action WinGameOver_Event;
		public event Action LoseGameOver_Event;
		
		#region Netcode Lifecycle
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerJoin;
			NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerLeave;
			NetworkManager.Singleton.OnConnectionEvent         += SingletonOnOnConnectionEvent;
			
			if (uiManager == null)
				uiManager = FindObjectOfType<UIManager>();

			// Server only from now
			if (!IsServer) return;
			
			InitializeLevel();
			
			// order of fucking operation.
			// now start flow only after UI's NetObject is spawned (or timeout)
			if (startFlowCoroutine != null) StopCoroutine(startFlowCoroutine);
			startFlowCoroutine = StartCoroutine(StartFlowWhenUIReady());
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
			{
				NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerJoin;
				NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerLeave;
			}

			if (!IsServer) return; // server side cleaning up from now on
			if (mothershipBases != null)
			{
				foreach (var mothershipBase in mothershipBases)
					mothershipBase.AlienSpawned_Event -= MothershipBaseOnAlienSpawned_Event;
			}

			if (civilians != null)
			{
				foreach (var civ in civilians)
				{
					var health = civ.GetComponent<Health>();
					if(health != null) health.OnDeath -= OnCivDeath;
				}
			}
		}
		
		public void OnLevelLoaded()
		{
			if (!IsServer) return;
			InitializeLevel();
		}
		
		private IEnumerator StartFlowWhenUIReady()
		{
			float t = 0f;
			while ((uiManager == null || !uiManager.TryGetComponent(out NetworkObject no) || !no.IsSpawned) && t < 3f)
			{
				if (uiManager == null) uiManager = FindObjectOfType<DanniLi.UIManager>();
				yield return null;
				t += Time.deltaTime;
			}

			// Initialize UI on server now that it's spawned
			if (uiManager != null && uiManager.IsSpawned)
				uiManager.InitializeUI(totalCivilians, civiliansAlive, totalWaves);

			// NOW start the first wave
			StartWave();
		}

		private void InitializeLevel()
		{
				//--------------------------------------------------LEVEL INFO-------------------------------------------------------------
			// TODO: Probably keep levelinfo a SO file, as we need the ability to show level names/info BEFORE the level is loaded
			currentlevelInfo = (levelSOs != null && levelSOs.Length > 0) ? levelSOs[Mathf.Clamp(levelSOs.Length, 0, levelSOs.Length - 1)] : null;
			if (currentlevelInfo != null)
			{
				Debug.Log("Level Info: Civilians to save" + currentlevelInfo.civiliansToSave);
			}
			else
			{
				Debug.Log("Level Info: No level info found");
			}
			//--------------------------------------------------CRATES & ITEMS-------------------------------------------------------------
			Crate[] crates = FindObjectsByType<Crate>(FindObjectsSortMode.None);
			if (levelSOs != null && levelSOs.Length > 0)
				foreach (Crate crate in crates)
				{
					LevelInfo levelSO                         = levelSOs[currentLevelIndex];
					if (levelSO != null) crate.availableItems = levelSO.availableItems;
				}
			else
			{
				Debug.LogWarning("GameManager: no level info!");
			}
			//--------------------------------------------------MOTHERSHIP & ALIENS-------------------------------------------------------------
			mothershipBases = FindObjectsByType<MothershipBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
			aliensIncomingFromAllShips = 0;
			foreach (MothershipBase mothershipBase in mothershipBases)
			{
				aliensIncomingFromAllShips        += mothershipBase.totalAlienSpawnCount;
				mothershipBase.AlienSpawned_Event += MothershipBaseOnAlienSpawned_Event;
			}
			Debug.Log("Aliens Incoming: " + aliensIncomingFromAllShips);
			
			//--------------------------------------------------CIVILIANS-------------------------------------------------------------
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
			//--------------------------------------------------GAME FLOW-------------------------------------------------------------
			if (uiManager != null)
			{
				uiManager.InitializeUI(totalCivilians, civiliansAlive, totalWaves);
			}
			// TODO coroutine to space it out
			GetReady_Event?.Invoke();
			// StartWave();
			StartGame_Event?.Invoke();
		}
		
		public void ForceInitializeUI(UIManager ui)
		{
			if (ui != null && ui.IsSpawned)
			{
				ui.InitializeUI(totalCivilians, civiliansAlive, totalWaves);
				if (waveInProgress)
					ui.OnWaveStart(currentWaveNumber);
				else
					ui.OnWaveEnd(currentWaveNumber);
			}
		}

		#endregion
		
		#region Alien Events
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
			if (uiManager != null)
				uiManager.OnAlienKilled();
			GameObject[] aliens = GameObject.FindGameObjectsWithTag("Alien");

			if (aliens.Length <= 0)
			{
				Debug.Log("Game Over: Win");
				WinGameOver_Event?.Invoke();
			}
		}

		private bool AreWavesOver()
		{
			return (aliensSpawnedSoFar >= totalAliensPlanned) &&
			       (aliensDeadSoFar >= totalAliensPlanned);
		}
		#endregion
		
		#region Civilian Events

		private void OnCivDeath()
		{
			civiliansAlive--;
			if (uiManager != null)
				uiManager.OnCivilianDeath(civiliansAlive);
			// Game over. Too many civs dead
			// if (civiliansAlive < civilians.Length * (currentlevelInfo.percentageToSave / 100f))
			if (civiliansAlive < civilians.Length - civiliansAlive) // gameover condition
			{
				Debug.Log("Game Over: Loss");
				LoseGameOver_Event?.Invoke();
				// winScreen.SetActive(false);
				// loseScreen.SetActive(true);
			}
		}
		#endregion
		
		#region Gameplay Flow

		// to decide end-of-waves and evaluate win condition
		private void TryEndWavesAndScore()
		{
			if(!IsServer) return;
			if(!AreWavesOver()) return;
			if (totalCivilians <= 0)
			{
				DoLose();
				return;
			}

			int required = (currentlevelInfo != null) ? currentlevelInfo.percentCiviliansAliveToWin : 50;
			float alivePercentage = (civiliansAlive / (float)totalCivilians) * 100f;
			if (alivePercentage >= required)
				DoWin();
			else DoLose();
		}

		private void DoWin()
		{
			Debug.Log("Game Win");
			WinGameOver_Event?.Invoke();
		}

		private void DoLose()
		{
			Debug.Log("Game Lost");
			LoseGameOver_Event?.Invoke();
		}
		#endregion
		
		#region Camera Control
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
			if (targetGroup == null) return;
			List<CinemachineTargetGroup.Target> targets = targetGroup.m_Targets.ToList();
			targets.RemoveAll(target => target.Object == playerTransform);
			targetGroup.m_Targets = targets.ToArray();
		}
		
		# endregion
		
		#region Utility Methods
		
		public void TestLoadScene()
		{
			SceneHelper.LoadScene("Main Menu", true, true);
		}

		public void TestUnloadScene()
		{
			SceneHelper.UnloadScene("Main Menu");
		}
		#endregion
		
		#region Spawners

		private void StartWave()
		{
			currentWaveNumber++;

			if (uiManager == null) uiManager = FindObjectOfType<DanniLi.UIManager>();
			var uiNO = uiManager ? uiManager.GetComponent<NetworkObject>() : null;

			Debug.Log($"[GM][SERVER] StartWave {currentWaveNumber} | UI isNull? {uiManager==null} | UI NO spawned? {uiNO && uiNO.IsSpawned} | id={(uiNO ? uiNO.NetworkObjectId : 0)}");

			if (uiManager != null)
				uiManager.OnWaveStart(currentWaveNumber);

			foreach (var spawner in spawners) spawner.StartWaves();
			Invoke(nameof(EndWave), 30f);
		}

		private void EndWave()
		{
			// noyify UI of wave end
			if (uiManager != null)
				uiManager.OnWaveEnd(currentWaveNumber);
			// check if more waves should start
			if (currentWaveNumber < totalWaves)
			{
				Invoke(nameof(StartWave), 5f); // start next wave after 5 seconds
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
		#endregion
	}
}