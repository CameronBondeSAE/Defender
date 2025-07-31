using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item")]
public class ItemSO : ScriptableObject
{
    [Header("Basic Item Info")] 
    public string Name;
    // in case you guys what to display item info on UI
    public string Description;
    public Sprite Icon;

    [Header("Item Prefab")] 
    public GameObject ItemPrefab;
}
