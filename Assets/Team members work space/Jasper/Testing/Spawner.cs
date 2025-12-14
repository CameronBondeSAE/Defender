using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Jasper_AI
{
    public class Spawner : NetworkBehaviour
    {
        [SerializeField] private List<GameObject> toSpawn = new List<GameObject>();
        [SerializeField] private int spawnCount;
        [SerializeField] private Vector3 areaTopCorner;
        [SerializeField] private Vector3 areaBottomCorner;

        public override void OnNetworkSpawn()
        {
            Vector3 spawnLocation;

            foreach (GameObject go in toSpawn)
            {
                spawnLocation = new Vector3(Random.Range(areaBottomCorner.x, areaTopCorner.x),
                    0, Random.Range(areaBottomCorner.z, areaTopCorner.z));

                for (int i = 0; i < spawnCount; i++)
                {
                    Instantiate(go, spawnLocation, Quaternion.identity);
                }
            }
        }
    }
}
