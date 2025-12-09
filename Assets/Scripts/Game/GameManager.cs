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
		// [SerializeField]
		// public LevelInfo[] levelSOs;

		[SerializeField]
		private LevelLoader levelLoader;
		
		[Header("Debugging: Don't edit")]
		private LevelInfo_SO currentlevelInfoSo;
		public int currentLevelIndex = 0;

		public LevelInfo levelInfo;
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
		private int aliensPerWaveFromAllShips;
		
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
		
		[Header("Crates")]
		[SerializeField] private List<CrateSpawn> crateSpawnpointsInScene = new List<CrateSpawn>();
		private List<NetworkObject> spawnedCrates = new(); 
		private int cratesSpawnedCount = 0;                         
		private int nextCrateSpawnIndex = 0; 
		
		[Header("Alien Eggs")]
		[SerializeField] private List<Transform> eggSpawnPos = new List<Transform>();

		[Tooltip("seconds after start of the game at which the eggs will begin to hatch")]
		[SerializeField] private float eggHatchStartDelay = 60f;
		[Tooltip("seconds between each egg hatching.")]
		[SerializeField] private float eggHatchInterval = 10f;

		private readonly List<Egg> spawnedEggs = new List<Egg>();
		private Coroutine eggHatchCoroutine;
		
		[Header("Game State")]
		private bool gameHasEnded = false;

		// Events
		public event Action GetReady_Event;
		public event Action StartGame_Event;
		public event Action WinGameOver_Event;
		public event Action LoseGameOver_Event;

		private IEnumerator Start()
		{
			if (IsServer)
			{
				while (!gameHasEnded)
				{
					CheckIfNoAliensLeft();
					yield return new WaitForSeconds(1f);
				}
			}
		}

		
		#region Netcode Lifecycle
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			var networkManager = NetworkManager;
			if (networkManager != null)
			{
				NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerJoin;
				NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerLeave;
				NetworkManager.Singleton.OnConnectionEvent         += SingletonOnOnConnectionEvent;
			}
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
		
		public override void OnNetworkDespawn()
		{
			var networkManager = NetworkManager;
			if (networkManager != null)
			{
				networkManager.OnClientConnectedCallback -= OnPlayerJoin;                
				networkManager.OnClientDisconnectCallback -= OnPlayerLeave;               
				networkManager.OnConnectionEvent         -= SingletonOnOnConnectionEvent; 
			}
			if (IsServer)
			{
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
						if (health != null) health.OnDeath -= OnCivDeath;
					}
				}
			}

			base.OnNetworkDespawn();
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
				uiManager.InitializeUI(totalCivilians, civiliansAlive, totalWaves, totalAliensPlanned);
			
			// spawn crates!
			SpawnInitialCrateForHost();

			// NOW start the first wave
			StartWave();
		}

		private void InitializeLevel()
		{
			//--------------------------------------------------LEVEL INFO-------------------------------------------------------------
			// ticked off the todos
			currentlevelInfoSo = (levelLoader.levelOrder != null && levelLoader.levelOrder.Length > 0)
				? levelLoader.levelOrder[Mathf.Clamp(currentLevelIndex, 0, levelLoader.levelOrder.Length - 1)] 
				: null;

			if (currentlevelInfoSo != null)
			{
				// Debug.Log("Level Info: Civilians to save " + currentlevelInfo.civiliansToSave);
			}
			else
			{
				Debug.Log("Level Info: No level info found");
			}
		
			//--------------------------------------------------CRATES & ITEMS-------------------------------------------------------------
			crateSpawnpointsInScene = FindObjectsByType<CrateSpawn>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).ToList();
			
			Crate[] crates = FindObjectsByType<Crate>(FindObjectsSortMode.None);
			if (levelLoader.levelOrder != null && levelLoader.levelOrder.Length > 0)
				foreach (Crate crate in crates)
				{
					LevelInfo_SO levelSO                         = levelLoader.levelOrder[currentLevelIndex];
					if (levelSO != null) crate.availableItems = levelSO.availableItems;
				}
			else
			{
				Debug.LogWarning("GameManager: no level info!");
			}

			//--------------------------------------------------MOTHERSHIP & ALIENS-------------------------------------------------------------
			mothershipBases = FindObjectsByType<MothershipBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
			// aliensIncomingFromAllShips = 0;
			aliensPerWaveFromAllShips = 0;
			foreach (MothershipBase mothershipBase in mothershipBases)
			{
				aliensIncomingFromAllShips        += mothershipBase.totalAlienSpawnCount;
				mothershipBase.AlienSpawned_Event += MothershipBaseOnAlienSpawned_Event;
			}
			totalAliensPlanned = aliensIncomingFromAllShips; 
			// Debug.Log("Aliens Incoming: " + aliensIncomingFromAllShips);
			
			//--------------------------------------------------CIVILIANS-------------------------------------------------------------
			civilians      = GameObject.FindGameObjectsWithTag("Civilian");
			totalCivilians = civilians.Length;
			civiliansAlive = civilians.Length;
			// Debug.Log("Civilians Alive: " + civiliansAlive);
			foreach (GameObject civilian in civilians)
			{
				Health health = civilian.GetComponent<Health>() 
				                ?? civilian.GetComponentInChildren<Health>();

				if (health != null)
				{
					health.OnDeath += OnCivDeath;
				}
			}
			
			//--------------------------------------------------EGGS-------------------------------------------------------------
			SpawnEggsForLevel();

			// kill any previous egg coroutines (like if reloaded level)
			if (eggHatchCoroutine != null)
			{
				StopCoroutine(eggHatchCoroutine);
				eggHatchCoroutine = null;
			}

			if (spawnedEggs.Count > 0)
			{
				eggHatchCoroutine = StartCoroutine(EggHatchRoutine());
			}
			
			//--------------------------------------------------GAME FLOW-------------------------------------------------------------
			if (uiManager != null)
			{
				uiManager.InitializeUI(totalCivilians, civiliansAlive, totalWaves, totalAliensPlanned);
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
				// recalculate total aliens planned in case it wasn't
				if (totalAliensPlanned <= 0)
					totalAliensPlanned = aliensPerWaveFromAllShips * totalWaves;
				uiManager.InitializeUI(totalCivilians, civiliansAlive, totalWaves, totalAliensPlanned);
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
			// now updates the total aliens planned dynamically as they spawn
			// because it seems like more aliens are spawned than planned/calculated from the mothership
			aliensSpawnedSoFar++;
			// if not spawning more aliens than planned, update the total
			if (aliensSpawnedSoFar > totalAliensPlanned)
			{
				totalAliensPlanned = aliensSpawnedSoFar + (aliensPerWaveFromAllShips * (totalWaves - currentWaveNumber));
            
				// Update UI with new total
				if (uiManager != null && uiManager.IsSpawned)
					uiManager.UpdateTotalAliens(totalAliensPlanned);
				Debug.Log($"updated total aliens planned to: {totalAliensPlanned}");
			}
			// update UI with current total
			if (uiManager != null && uiManager.IsSpawned)
				uiManager.UpdateAlienProgress(aliensSpawnedSoFar, totalAliensPlanned);
		}
		private void OnAlienDeath()
		{
			aliensDeadSoFar++;

			if (uiManager != null)
				uiManager.OnAlienKilled();
			CheckIfNoAliensLeft();
		}
		
		public void OnAlienLeftLevel()
		{
			if (!IsServer) return;
			CheckIfNoAliensLeft();
		}

		private bool AreWavesOver()
		{
			return (aliensSpawnedSoFar >= totalAliensPlanned) &&
			       (aliensDeadSoFar >= totalAliensPlanned);
		}
		#endregion
		
		#region Civilian Events

		public void OnCivDeath()
		{
			civiliansAlive = Mathf.Max(0, civiliansAlive - 1);
			Debug.Log($"[GM][SERVER] OnCivDeath -> Alive {civiliansAlive}/{totalCivilians}");
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
		public void OnCivAbducted()
		{
			if (!IsServer) return;
			civiliansAlive = Mathf.Max(0, civiliansAlive - 1);
			Debug.Log($"Civs alive {civiliansAlive}/{totalCivilians}");
			if (uiManager != null)
				uiManager.OnCivilianDeath(civiliansAlive);
		}

		#endregion
		
		#region Gameplay Flow
		
		private bool CheckMothershipsFinishedWaves()
		{
			if (mothershipBases == null || mothershipBases.Length == 0)
				return true; 
			for (int i = 0; i < mothershipBases.Length; i++)
			{
				var ms = mothershipBases[i];
				if (ms != null && !ms.AllWavesFinished)
				{
					return false;
				}
			}
			return true;
		}
		
		private void CheckIfNoAliensLeft()
		{
			if (!IsServer || gameHasEnded) return;
			// only check if all waves are finished
			if (!CheckMothershipsFinishedWaves()) return;
			GameObject[] aliens = GameObject.FindGameObjectsWithTag("Alien");
			if (aliens.Length > 0)
			{
				return;
			}
			TryEndWavesAndScore();
		}

		// to decide end-of-waves and evaluate win condition
		private void TryEndWavesAndScore()
		{
			if(!IsServer) return;
			// if(!AreWavesOver()) return;
			if (totalCivilians <= 0)
			{
				DoLose();
				return;
			}
			int required = (currentlevelInfoSo != null) ? currentlevelInfoSo.percentCiviliansAliveToWin : 50;
			int requiredAliveCount = Mathf.CeilToInt((required / 100f) * totalCivilians); // rounded up
			if (civiliansAlive >= requiredAliveCount)
			{
				DoWin();
			}
			else
			{
				DoLose();
			}
		}

		public void DoWin()
		{
			if (gameHasEnded) return;
			gameHasEnded = true;
			Debug.Log("Game Win");
			WinGameOver_Event?.Invoke();
		}

		public void DoLose()
		{
			if (gameHasEnded) return;
			gameHasEnded = true;
			Debug.Log("Game Lost");
			LoseGameOver_Event?.Invoke();
		}
		#endregion
		
		#region Camera Control
		// Add player to target group when they join
		public void OnPlayerJoin(ulong playerID)
		{
			// Debug.Log("Player joined called " + playerID);
			if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerID, out NetworkClient client))
			{
				// Debug.Log("Player joined: " + client.PlayerObject.name);
				if (client.PlayerObject.IsLocalPlayer)
				{
					AddItemToCameraTargetGroup(client.PlayerObject.transform);
				}
			}
			
			if (IsServer)
			{
				if (playerID == NetworkManager.ServerClientId) return; // to fix the double spawn issue in build
				TrySpawnCrateForNewClient(); // forgive me for putting this in your camera code, just sharing this function
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

			// Debug.Log($"[GM][SERVER] StartWave {currentWaveNumber} | UI isNull? {uiManager==null} | UI NO spawned? {uiNO && uiNO.IsSpawned} | id={(uiNO ? uiNO.NetworkObjectId : 0)}");

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
		

		private Vector3 GetRandomSpawnPosition()
		{
			return new Vector3(Random.Range(-50, 50), -0.5f, Random.Range(-50, 50)); // adjust these based on level size
		}
		#endregion
		
		#region Crates
		 private void SpawnInitialCrateForHost() 
    {
        if (!IsServer || currentlevelInfoSo == null) return;
        int maxCrates = Mathf.Max(0, currentlevelInfoSo.crateSpawnCount);
        int connectedClients = NetworkManager.Singleton.ConnectedClientsIds.Count;
        int desired = Mathf.Min(maxCrates, connectedClients);
        while (cratesSpawnedCount < desired)
        {
            TrySpawnCrateForNewClient();
        }
    }
    // Spawns one crate at the next spawn point from LevelInfo,
    // added functionality to spawn at random position if no spawnPos configured (also as an alternative :3)
    private void TrySpawnCrateForNewClient() 
    {
        if (!IsServer || currentlevelInfoSo == null) return;
        
	    // CAM HACK: TODO
	    levelInfo = FindAnyObjectByType<LevelInfo>();
        
        int maxCrates = Mathf.Max(0, currentlevelInfoSo.crateSpawnCount);
        if (cratesSpawnedCount >= maxCrates) return;
        if (currentlevelInfoSo.cratePrefab == null) return;
        // choose spawn transform in listed order
        Transform spawnPos = null;
		// now prefer scene-assigned spawn points on this manager 
        if (crateSpawnpointsInScene != null &&
            nextCrateSpawnIndex < crateSpawnpointsInScene.Count)
        {
	        spawnPos = crateSpawnpointsInScene[nextCrateSpawnIndex].transform;
        }
		// if null, fallback to LevelInfo prefab spawn points
        else if (levelInfo != null &&
                 levelInfo.crateSpawns != null &&
                 nextCrateSpawnIndex < levelInfo.crateSpawns.Count)
        {
	        spawnPos = levelInfo.crateSpawns[nextCrateSpawnIndex].transform;
        }

        Vector3 pos = (spawnPos != null) ? spawnPos.position : GetRandomSpawnPosition();
        Quaternion rotation = (spawnPos != null) ? spawnPos.rotation : Quaternion.identity;
        var crateGO = Instantiate(currentlevelInfoSo.cratePrefab, pos, rotation, levelContainer);
        var netObj = crateGO.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Destroy(crateGO);
            return;
        }
        netObj.Spawn(); 
        spawnedCrates.Add(netObj);
        cratesSpawnedCount++;
        if (spawnPos != null) nextCrateSpawnIndex++; // spawn at next spawnPos in list
    }
		#endregion
		
		#region Eggs
		private void SpawnEggsForLevel()
		{
			if (!IsServer) return;
			if (currentlevelInfoSo == null)
				return;
			
			spawnedEggs.Clear();

			if (eggSpawnPos == null || eggSpawnPos.Count == 0)
				return;

			if (currentlevelInfoSo.eggPrefabs == null || currentlevelInfoSo.eggPrefabs.Count == 0)
			{
				Debug.Log("no egg prefabs configured in LevelInfo; skipping egg spawn.");
				return;
			}

			for (int i = 0; i < eggSpawnPos.Count; i++)
			{
				Transform spawnPoint = eggSpawnPos[i];
				if (spawnPoint == null) continue;

				// choose an egg prefab at random
				GameObject eggPrefab = currentlevelInfoSo.eggPrefabs[0];
				if (currentlevelInfoSo.eggPrefabs.Count > 1)
				{
					int randomIndex = UnityEngine.Random.Range(0, currentlevelInfoSo.eggPrefabs.Count);
					eggPrefab = currentlevelInfoSo.eggPrefabs[randomIndex];
				}

				GameObject eggInstance = Instantiate(eggPrefab, spawnPoint.position, spawnPoint.rotation, levelContainer);
				NetworkObject eggNetObj = eggInstance.GetComponent<NetworkObject>();
				if (eggNetObj != null)
				{
					eggNetObj.Spawn();
				}
				Egg eggComponent = eggInstance.GetComponent<Egg>();
				if (eggComponent != null)
				{
					spawnedEggs.Add(eggComponent);
				}
			}
		}
		private IEnumerator EggHatchRoutine()
		{
			if (spawnedEggs.Count == 0)
				yield break;
			if (eggHatchStartDelay > 0f)
				yield return new WaitForSeconds(eggHatchStartDelay);

			for (int i = 0; i < spawnedEggs.Count; i++)
			{
				Egg egg = spawnedEggs[i];

				if (egg != null && egg.IsSpawned)
				{
					egg.Hatch();
				}

				if (eggHatchInterval > 0f && i < spawnedEggs.Count - 1)
				{
					yield return new WaitForSeconds(eggHatchInterval);
				}
			}
		}

		#endregion
	}
}