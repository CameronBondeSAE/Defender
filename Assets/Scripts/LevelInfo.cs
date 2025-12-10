using System.Collections.Generic;
using UnityEngine;

namespace mothershipScripts
{
	public class LevelInfo : MonoBehaviour
	{
		[Header("Win Conditions")]
		[Range(0, 100)]
		[Tooltip("percentage of civilians that must be alive at the end of the level to win")]
		public int percentCiviliansAliveToWin = 50;

		[Header("Spawners")]
		[Tooltip("all spawner GOs in this level that implement ISpawner (motherships)")]
		public List<ISpawner> spawnerComponents = new List<ISpawner>();

		[Header("Alien & Wave Settings")]
		[Tooltip("motherships in this level.")]
		public MothershipBase[] mothershipBases;
		[Tooltip("total waves for this level.")]
		public int totalWaves = 3;
		[Tooltip("enemy count multiplier for this level.")]
		public float enemyMult = 1f;
		[Tooltip("wave speed multiplier for this level.")]
		public float waveMult = 1f;

		[Header("Crate Settings")]
		[Tooltip("crate spawn points for this level. must have CrateSpawn component on them")]
		public List<CrateSpawn> crateSpawns = new List<CrateSpawn>();

		[Header("Alien Egg Settings")]
		public List<Transform> eggSpawnPositions = new List<Transform>();
		[Tooltip("eggs will hatch this many seconds after game/level starts")]
		public float eggHatchStartDelay = 60f;
		[Tooltip("seconds between each egg hatching")]
		public float eggHatchInterval = 10f;
	}
}