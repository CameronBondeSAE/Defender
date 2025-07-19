using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item")]
public class ItemSO : ScriptableObject
{
    [Header("Basic Item Info")]
    [field: SerializeField]
    public string Name { get; set; }

    [field: SerializeField]
    [field: TextArea]
    public string Description { get; set; }

    [field: SerializeField]
    public Sprite Icon { get; set; }

    [Header("Item Prefab")]
    [field: SerializeField]
    public GameObject ItemPrefab { get; set; }
}
public enum UIInteractionType
{
    TransitionScene,
    PickUpItem,
    CustomAction
}

