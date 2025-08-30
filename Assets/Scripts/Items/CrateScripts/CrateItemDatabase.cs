using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crate Item Database", menuName = "Defender/Crate/Item Database")]
public class CrateItemDatabase : ScriptableObject
{
    [Header("Level Configuration")]
    [Tooltip("The level this database is for")]
    public string levelName;
    
    [Header("Available Items to Spawn")]
    public List<GameObject> itemPrefabs = new List<GameObject>();
    public GameObject GetRandomItemPrefab()
    {
        if (itemPrefabs == null || itemPrefabs.Count == 0)
        {
            return null;
        }
        
        int randomIndex = Random.Range(0, itemPrefabs.Count);
        return itemPrefabs[randomIndex];
    }
}
