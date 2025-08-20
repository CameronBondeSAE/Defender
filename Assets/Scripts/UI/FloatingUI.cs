using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class FloatingUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField]
    private Sprite defaultIcon; // one icon for all pickups now
    
    [SerializeField]
    private Transform iconHolder;
    
    [SerializeField]
    private Camera mainCamera;

    [Header("Animation Settings")]
    [SerializeField]
    private float floatHeight = 0.5f;
    
    [SerializeField]
    private float floatDuration = 1f;

    private Image uiIcon;
    private Canvas iconCanvas;

    GameObject iconGO;

    private void Start()
    {
        // Get main cam
        if (!mainCamera) 
            mainCamera = Camera.main;
        
        SetupIcon();
        AnimateFloating();
    }

    private void Update()
    {
        FaceCamera();
    }

    public void Enable()
    {
        iconGO.SetActive(true);
    }

    public void Disable()
    {
        iconGO.SetActive(false);
    }

    private void SetupIcon()
    {
	    if (defaultIcon && iconHolder)
        {
            // Create icon GameObject
            iconGO = new GameObject("UIIcon");
            iconGO.transform.SetParent(iconHolder);
            iconGO.transform.localPosition = Vector3.zero;
            iconGO.transform.localScale    = Vector3.one;
            
            uiIcon        = iconGO.AddComponent<Image>();
            uiIcon.sprite = defaultIcon;

            // Add Canvas for world space rendering
            iconCanvas            = iconGO.AddComponent<Canvas>();
            iconCanvas.renderMode = RenderMode.WorldSpace;
            var rectTransform = iconGO.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(1f, 1f);
            //iconGO.AddComponent<GraphicRaycaster>();
        }
    }

    private void AnimateFloating()
    {
        transform.DOMoveY(transform.position.y + floatHeight, floatDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void FaceCamera()
    {
        if (iconHolder && mainCamera)
        {
            iconHolder.LookAt(mainCamera.transform);
            iconHolder.Rotate(0, 180, 0); 
        }
    }

    /// <summary>
    /// Call this when the item is picked up to handle cleanup
    /// </summary>
    public void OnPickedUp()
    {
        // Stop any tweens
        transform.DOKill();
        
        // pick-up animation
        transform.DOScale(0f, 0.2f).OnComplete(() => {
            Destroy(gameObject);
        });
    }
}
