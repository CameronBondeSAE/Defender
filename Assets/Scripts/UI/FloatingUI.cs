using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Inventory.SO;
using Inventory;

[RequireComponent(typeof(Collider))]
public class FloatingUI : MonoBehaviour
{
    public ItemSO itemData;
    private PlayerInputHandler playerInput;
    public Transform iconHolder;
    public Camera mainCamera;

    public UnityEvent onCustomAction;

    private bool playerInRange = false;
    private bool hasInteracted = false;
    private Image uiIcon;
    
    private InventoryController inventoryController;

    void Start()
    {
        if (!mainCamera) mainCamera = Camera.main;
        inventoryController = FindObjectOfType<Inventory.InventoryController>();
        AnimateFloating();
        SetupIcon();
    }

    void Update()
    {
        FaceCamera();
    }

    private void SetupIcon()
    {
        if (itemData && itemData.icon && iconHolder)
        {
            var iconGO = new GameObject("UIIcon");
            iconGO.transform.SetParent(iconHolder);
            iconGO.transform.localPosition = Vector3.zero;

            uiIcon = iconGO.AddComponent<Image>();
            uiIcon.sprite = itemData.icon;

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
        if (itemData == null) return;

        switch (itemData.interactionType)
        {
            case UIInteractionType.TransitionScene:
                if (!string.IsNullOrEmpty(itemData.sceneToLoad))
                {
                    SceneManager.LoadScene(itemData.sceneToLoad);
                }
                break;

            case UIInteractionType.PickUpItem:
                if (inventoryController != null && itemData != null)
                {
                    inventoryController.AddItemToInventory(itemData);
                }
                Destroy(gameObject);
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
