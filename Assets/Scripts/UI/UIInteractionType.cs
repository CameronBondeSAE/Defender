using UnityEngine;

[CreateAssetMenu(menuName = "UI/Interaction Data")]
public class UIInteractionData : ScriptableObject
{
    public UIInteractionType interactionType;
    public string promptText;
    public string sceneToLoad;
    public Sprite icon;
}

public enum UIInteractionType
{
    TransitionScene,
    PickUpItem,
    CustomAction
}

