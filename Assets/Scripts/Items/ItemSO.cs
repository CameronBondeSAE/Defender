using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Defender/Item/Items")]
public class ItemSO : ScriptableObject
{
    [Header("Basic Item Info")] 
    public string Name;
    // in case you guys what to display item info on UI
    public string Description;
    public Sprite BackingIcon; // Mainly for legendary items

    [Header("Item Prefab")] 
    public GameObject ItemPrefab;
    public GameObject ItemCrateUIPrefab;
}
