using System.Collections.Generic;
using UnityEngine;

namespace DanniLi
{

	[CreateAssetMenu(fileName = "New Level Info", menuName = "Defender/Level/Level Info")]
	public class LevelInfo_SO : ScriptableObject
	{
		[Header("Scene")] 
		public string sceneName;
		[Header("Level Items")]
		[SerializeField]
		public List<ItemSO> availableItems = new List<ItemSO>();
		public Object scene;
		[Header("Crate Objects")]
		[SerializeField] public GameObject cratePrefab;
		[SerializeField] public int crateSpawnCount = 5;
		[Header("Egg Objects")]
		[Tooltip("Different types of egg prefabs this level will have, they spawn smart planer AIs")]
		[SerializeField] public List<GameObject> eggPrefabs = new List<GameObject>();
	}
}