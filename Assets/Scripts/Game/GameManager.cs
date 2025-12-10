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
		
		[Header("Player Setup")]
		[SerializeField] private GameObject playerPrefab;

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

		
		#region Netcode Lifecycle (Start Game Logic)
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
			if (levelInfo == null)
				levelInfo = FindObjectOfType<LevelInfo>();
			InitializeLevel();
			SpawnPlayersForNewLevel();
			int requiredPercent = 50;
			if (levelInfo != null)
			{
				requiredPercent = levelInfo.percentCiviliansAliveToWin;
			}

			requiredPercent = Mathf.Clamp(requiredPercent, 0, 100);

			// show the added level intro screen (also on clients)
			if (uiManager != null && uiManager.IsSpawned)
			{
				uiManager.ShowLevelIntroRpc(requiredPercent);
			}
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
			if (levelLoader != null && levelLoader.levelOrder != null && levelLoader.levelOrder.Length > 0)
			{
				currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, levelLoader.levelOrder.Length - 1);
				currentlevelInfoSo = levelLoader.levelOrder[currentLevelIndex];
			}
			else
			{
				currentlevelInfoSo = null;
				Debug.LogWarning("GM: No LevelInfo_SO");
			}
			levelInfo = FindAnyObjectByType<LevelInfo>();
			gameHasEnded = false;
			if (levelInfo != null)
			{
				// ISpawners
				spawners.Clear();
				if (levelInfo != null && levelInfo.mothershipBases != null && levelInfo.mothershipBases.Length > 0)
				{
					mothershipBases = levelInfo.mothershipBases;

					foreach (var ms in mothershipBases)
					{
						if (ms == null) continue;
						spawners.Add(ms); 
					}
				}
				else
				{
					mothershipBases = FindObjectsByType<MothershipBase>(
						FindObjectsInactive.Exclude, FindObjectsSortMode.None);

					foreach (var ms in mothershipBases)
					{
						if (ms == null) continue;
						spawners.Add(ms);
					}
				}

				// Wave & difficulty settings
				totalWaves = levelInfo.totalWaves;
				enemyMult  = levelInfo.enemyMult;
				waveMult   = levelInfo.waveMult;

				// Crates
				if (levelInfo.crateSpawns != null && levelInfo.crateSpawns.Count > 0)
				{
					crateSpawnpointsInScene = new List<CrateSpawn>(levelInfo.crateSpawns);
				}

				// Motherships
				if (levelInfo.mothershipBases != null && levelInfo.mothershipBases.Length > 0)
				{
					mothershipBases = levelInfo.mothershipBases;
				}

				// Eggs
				if (levelInfo.eggSpawnPositions != null && levelInfo.eggSpawnPositions.Count > 0)
				{
					eggSpawnPos = new List<Transform>(levelInfo.eggSpawnPositions);
				}
				eggHatchStartDelay = levelInfo.eggHatchStartDelay;
				eggHatchInterval   = levelInfo.eggHatchInterval;
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
				Debug.LogWarning("gameManager: no level info!");
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
			if (eggHatchCoroutine != null)
			{
				StopCoroutine(eggHatchCoroutine);
				eggHatchCoroutine = null;
			}
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
		
		[Rpc(SendTo.Server, RequireOwnership = false)]
		public void BeginLevelServerRpc()
		{
			if (!IsServer) return;

			// start game for this level now that players have pressed 'Start' in-level
			if (startFlowCoroutine != null)
			{
				StopCoroutine(startFlowCoroutine);
			}
			startFlowCoroutine = StartCoroutine(StartFlowWhenUIReady());
			StartEggHatchRoutine();
			// start game events to be used in the future
			GetReady_Event?.Invoke();
			StartGame_Event?.Invoke();
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
		
		#region Player
		private void SpawnPlayersForNewLevel()
		{
			if (!IsServer) return;
			if (NetworkManager.Singleton == null) return;

			if (playerPrefab == null)
				return;
			
			
			Transform spawnPoint = null;
			if (levelInfo != null && levelInfo.playerSpawnPoint != null)
			{
				spawnPoint = levelInfo.playerSpawnPoint;
			}

			if (spawnPoint == null)
				return;

			foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
			{
				ulong clientId = kvp.Key;
				NetworkClient client = kvp.Value;
				NetworkObject existing = client.PlayerObject;
				if (existing != null)
				{
					existing.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
					continue;
				}
				GameObject instance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
				if (instance == null)
				{
					continue;
				}

				var netObj = instance.GetComponent<NetworkObject>();
				if (netObj == null)
				{
					Destroy(instance);
					continue;
				}

				netObj.SpawnAsPlayerObject(clientId, true);
			}
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
			int required = 50;
			if (levelInfo != null)
			{
				required = levelInfo.percentCiviliansAliveToWin;
			}
			int requiredAliveCount = Mathf.CeilToInt((required / 100f) * totalCivilians); 

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
			if (IsServer && NetworkManager != null)
			{
				if (playerID != NetworkManager.ServerClientId) 
				{
					TrySpawnCrateForNewClient();
				}
			}
			
			StartCoroutine(WaitForLocalPlayerAndRegisterCamera(playerID));
		}
		
		private IEnumerator WaitForLocalPlayerAndRegisterCamera(ulong playerID)
		{
			float timeout = 5f;
			float elapsed = 0f;

			while (elapsed < timeout && NetworkManager.Singleton != null)
			{
				if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerID, out NetworkClient client))
				{
					var playerObj = client.PlayerObject;
					if (playerObj != null && playerObj.IsLocalPlayer)
					{
						AddItemToCameraTargetGroup(playerObj.transform);
						yield break;
					}
				}

				elapsed += Time.deltaTime;
				yield return null;
			}

			Debug.LogWarning($"[GM] WaitForLocalPlayerAndRegisterCamera timed out for client {playerID}");
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
				newTarget.Weight = 1f; 
				newTarget.Radius = 1f; 
		
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
	    if (!IsServer)
	        return;

	    if (currentlevelInfoSo == null)
	    {
	        Debug.LogWarning("currentlevelInfoSo is null");
	        return;
	    }

	    if (currentlevelInfoSo.cratePrefab == null)
	    {
	        Debug.LogWarning("cratePrefab is null on LevelInfo_SO.");
	        return;
	    }
	    
	    cratesSpawnedCount   = 0;
	    spawnedCrates.Clear();
	    nextCrateSpawnIndex  = 0;

	    int maxCrates        = Mathf.Max(0, currentlevelInfoSo.crateSpawnCount);
	    int connectedClients = NetworkManager.Singleton != null
	        ? NetworkManager.Singleton.ConnectedClientsIds.Count
	        : 1;

	    // one crate per client (up to maxCrates)
	    int desired = Mathf.Min(maxCrates, connectedClients);

	    for (int i = 0; i < desired; i++)
	    {
	        TrySpawnCrateForNewClient();
	    }
	}

	// if no spawn points exist, use random position
	private void TrySpawnCrateForNewClient()
	{
	    if (!IsServer)
	        return;

	    if (currentlevelInfoSo == null)
	        return;

	    if (currentlevelInfoSo.cratePrefab == null)
	        return;

	    int maxCrates = Mathf.Max(0, currentlevelInfoSo.crateSpawnCount);
	    if (cratesSpawnedCount >= maxCrates)
	        return;
	    
	    Transform spawnPos = null;
	    
	    if (crateSpawnpointsInScene != null && crateSpawnpointsInScene.Count > 0)
	    {
	        int index = nextCrateSpawnIndex % crateSpawnpointsInScene.Count;
	        spawnPos  = crateSpawnpointsInScene[index].transform;
	        nextCrateSpawnIndex++;
	    }
	    else if (levelInfo != null && levelInfo.crateSpawns != null && levelInfo.crateSpawns.Count > 0)
	    {
	        int index = nextCrateSpawnIndex % levelInfo.crateSpawns.Count;
	        spawnPos  = levelInfo.crateSpawns[index].transform;
	        nextCrateSpawnIndex++;
	    }

	    Vector3 position = spawnPos != null ? spawnPos.position : GetRandomSpawnPosition();
	    Quaternion rotation = spawnPos != null ? spawnPos.rotation : Quaternion.identity;

	    // actually spawn the crate
	    GameObject crateInstance = Instantiate(currentlevelInfoSo.cratePrefab, position, rotation, levelContainer);
	    NetworkObject crateNetObj = crateInstance.GetComponent<NetworkObject>();
	    if (crateNetObj != null)
	    {
	        crateNetObj.Spawn();
	        spawnedCrates.Add(crateNetObj);
	    }

	    cratesSpawnedCount++;
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
		
		private void StartEggHatchRoutine()
		{
			if (!IsServer) return;
			if (eggHatchCoroutine != null)
			{
				StopCoroutine(eggHatchCoroutine);
				eggHatchCoroutine = null;
			}

			if (spawnedEggs.Count > 0)
			{
				eggHatchCoroutine = StartCoroutine(EggHatchRoutine());
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