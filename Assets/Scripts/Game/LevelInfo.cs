using System.Collections.Generic;
using UnityEngine;

namespace DanniLi
{

	[CreateAssetMenu(fileName = "New Level Info", menuName = "Level/Level Info")]
	public class LevelInfo : ScriptableObject
	{
		[Header("Scene")] 
		public string sceneName;
		
		[Header("Level Items")]
		[SerializeField]
		public List<ItemSO> availableItems = new List<ItemSO>();
		public Object scene;
		public int    civiliansToSave; // this is optional now

		[Header("Level Props")]
		[SerializeField]
		public GameObject wallPrefab;
		[SerializeField]
		public int wallSpawnCount = 10;
		[SerializeField]
		public GameObject cratePrefab;
		[SerializeField]
		public int crateSpawnCount = 5;

		[Header("Win Conditions")] [Range(0, 100)]
		public int percentCiviliansAliveToWin = 50;
		
		/// <summary>
		/// If needed, this returns a random item from the available items list:)
		/// </summary>
		public ItemSO GetRandomItem()
		{
			if (availableItems.Count == 0)
			{
				Debug.LogWarning("No items available in level info!");
				return null;
			}
			int randomIndex = Random.Range(0, availableItems.Count);
			return availableItems[randomIndex];
		}
	}
}