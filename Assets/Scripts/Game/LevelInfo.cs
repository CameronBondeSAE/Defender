using System.Collections.Generic;
using UnityEngine;

namespace DanniLi
{

	[CreateAssetMenu(fileName = "New Level Info", menuName = "Defender/Level/Level Info")]
	public class LevelInfo : ScriptableObject
	{
		[Header("Scene")] 
		public string sceneName;
		
		[Header("Level Items")]
		[SerializeField]
		public List<ItemSO> availableItems = new List<ItemSO>();
		public Object scene;
		public int    civiliansToSave; // this is optional now

		[Header("Crates")]
		[SerializeField] public GameObject cratePrefab;
		[SerializeField] public int crateSpawnCount = 5;
		// spawn points for crates (they will be spawned in list order)
		[SerializeField] public List<Transform> crateSpawnPoints = new List<Transform>();

		[Header("Win Conditions")] [Range(0, 100)]
		public int percentCiviliansAliveToWin = 50;
	}
}