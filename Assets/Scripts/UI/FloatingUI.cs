using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class FloatingUI : MonoBehaviour
{
    public UIInteractionData interactionData;
    private PlayerInputHandler playerInput;
    public Transform iconHolder;
    public Camera mainCamera;

    public UnityEvent onCustomAction;

    private bool playerInRange = false;
    private bool hasInteracted = false;
    private Image uiIcon;

    void Start()
    {
        if (!mainCamera) mainCamera = Camera.main;

        AnimateFloating();
        SetupIcon();
    }

    void Update()
    {
        FaceCamera();
    }

    private void SetupIcon()
    {
        if (interactionData && interactionData.icon && iconHolder)
        {
            var iconGO = new GameObject("UIIcon");
            iconGO.transform.SetParent(iconHolder);
            iconGO.transform.localPosition = Vector3.zero;

            uiIcon = iconGO.AddComponent<Image>();
            uiIcon.sprite = interactionData.icon;

            var canvas = iconGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var rectTransform = iconGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(1f, 1f);
        }
    }

    private void AnimateFloating()
    {
        transform.DOMoveY(transform.position.y + 0.5f, 1f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void FaceCamera()
    {
        if (iconHolder && mainCamera)
        {
            iconHolder.LookAt(mainCamera.transform);
        }
    }

    private void HandleInteraction()
    {
        if (interactionData == null) return;

        switch (interactionData.interactionType)
        {
            case UIInteractionType.TransitionScene:
                if (!string.IsNullOrEmpty(interactionData.sceneToLoad))
                {
                    SceneManager.LoadScene(interactionData.sceneToLoad);
                }
                break;

            case UIInteractionType.PickUpItem:
                //PromptUI.Instance.ShowPrompt(interactionData.promptText, interactionData.sceneToLoad);
                break;

            case UIInteractionType.CustomAction:
                onCustomAction?.Invoke();
                break;
        }

        hasInteracted = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerInput = other.GetComponent<PlayerInputHandler>();

            if (playerInput != null)
            {
                playerInput.interactAction.performed += OnInteractPerformed;
            }

            // Optional: Show "Press E" UI
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerInput = other.GetComponent<PlayerInputHandler>();

            if (playerInput != null)
            {
                playerInput.interactAction.performed -= OnInteractPerformed;
            }
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (playerInRange)
        {
            HandleInteraction();
        }
    }
}
