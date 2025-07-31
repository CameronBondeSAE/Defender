using System.Collections.Generic;
using mothershipScripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace DanniLi
{
	public class GameManager : MonoBehaviour
	{
		[Header("References")]
		[SerializeField]
		private LevelInfo levelInfo;

		private PlayerInventory playerInventory;

		[Header("Spawn Settings")]
		[SerializeField]
		private Transform levelContainer; // for organization

		[Header("Debugging: Don't edit")]
		public List<ISpawner> spawners = new List<ISpawner>();

		private void Start()
		{
			FindPlayerInventory();
			SetupPlayerInventoryItems();
			SpawnLevelProps();

			StartWave();
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

		private void FindPlayerInventory()
		{
			GameObject player = GameObject.FindGameObjectWithTag("Player");

			if (player == null)
			{
				return;
			}

			playerInventory = player.GetComponent<PlayerInventory>();
			if (playerInventory == null)
			{
				Debug.LogError("GameManager: please attach PlayerInventory to player!");
			}
		}

		private void SetupPlayerInventoryItems()
		{
			if (playerInventory == null)
			{
				return;
			}

			if (levelInfo == null)
			{
				return;
			}

			foreach (var item in levelInfo.availableItems)
			{
				if (item != null)
				{
					playerInventory.RegisterAvailableItem(item);
					Debug.Log($"GameManager: Registered item {item.name}");
				}
				else
				{
					Debug.LogWarning("GameManager: please assign items in LevelInfo!");
				}
			}
		}

		private void SpawnLevelProps()
		{
			if (levelInfo == null)
			{
				Debug.LogWarning("GameManager: no level info!");
				return;
			}

			for (int i = 0; i < levelInfo.wallSpawnCount; i++)
			{
				Instantiate(levelInfo.wallPrefab, GetRandomSpawnPosition(), Quaternion.identity, levelContainer);
			}

			for (int i = 0; i < levelInfo.crateSpawnCount; i++)
			{
				if (levelInfo.cratePrefab != null)
					Instantiate(levelInfo.cratePrefab, GetRandomSpawnPosition(), Quaternion.identity, levelContainer);
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