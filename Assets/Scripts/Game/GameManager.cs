using System.Collections.Generic;
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

		private void Awake()
		{
			FindPlayerInventory();
			SetupPlayerInventoryItems();
			SpawnLevelProps();
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
			for (int i = 0; i < levelInfo.wallSpawnCount; i++)
			{
				Instantiate(levelInfo.wallPrefab, GetRandomSpawnPosition(), Quaternion.identity, levelContainer);
			}

			for (int i = 0; i < levelInfo.crateSpawnCount; i++)
			{
				Instantiate(levelInfo.cratePrefab, GetRandomSpawnPosition(), Quaternion.identity, levelContainer);
			}
		}

		private Vector3 GetRandomSpawnPosition()
		{
			return new Vector3(Random.Range(-50, 50), -0.5f, Random.Range(-50, 50)); // adjust these based on level size
		}
	}
}