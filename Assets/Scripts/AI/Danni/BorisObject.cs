using Defender;
using DG.Tweening;
using UnityEngine;

public class BorisObject : MonoBehaviour, IUsable
{
    [SerializeField] private Boris boris;

    [Header("UI")]
    [SerializeField] private GameObject interactPromptUI;
    [SerializeField] private float floatAmplitude = 0.25f;
    [SerializeField] private float floatDuration = 1.5f;

    private Tween floatTween;
    private void Start()
    {
        if (interactPromptUI != null)
        {
            Transform t = interactPromptUI.transform;
            Vector3 startPos = t.localPosition;

            floatTween = t.DOLocalMoveY(startPos.y + floatAmplitude, floatDuration)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInputHandler2 playerInput = other.GetComponentInParent<PlayerInputHandler2>();
        if (playerInput == null || !playerInput.IsLocalPlayer)
            return;
        if (interactPromptUI != null)
        {
            interactPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerInputHandler2 playerInput = other.GetComponentInParent<PlayerInputHandler2>();
        if (playerInput == null || !playerInput.IsLocalPlayer)
        {
            return;
        }
        if (interactPromptUI != null)
        {
            interactPromptUI.SetActive(false);
        }
    }
    public void Use(CharacterBase user)
    {
        if (boris == null)
            return;
        
        PlayerInputHandler2 playerInput = user.GetComponent<PlayerInputHandler2>();
        if (playerInput == null || !playerInput.IsLocalPlayer)
            return;
        if (interactPromptUI != null)
        {
            interactPromptUI.SetActive(false);
        }
        boris.BeginConversation();
    }

    public void StopUsing()
    {
        
    }
}
