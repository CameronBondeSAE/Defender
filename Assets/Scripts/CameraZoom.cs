using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraZoom : MonoBehaviour
{
    public string playerTag = "Player";
    public bool updateContinuously = false;
    public CinemachineCamera vcam;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 120f;
    [SerializeField] private float minFOV = 80f;   // For perspective
    [SerializeField] private float maxFOV = 120f;   // For perspective
    private float targetSize;
    private float targetFOV;


    private void Awake()
    {
        vcam = GetComponent<CinemachineCamera>();
        targetSize = vcam.Lens.OrthographicSize;
    }

    private void OnEnable()
    {
        AssignFollowTarget();
    }

    private void Update()
    {
        CamZoom();
    }

    private void CamZoom()
    {
        float scrollValue = 0f;
        if (Mouse.current != null)
        {
            Vector2 scrollVector = Mouse.current.scroll.ReadValue();
            scrollValue = scrollVector.y;
        }
        
        if (scrollValue != 0)
        {
            float oldFOV = vcam.Lens.FieldOfView;
            float zoomChange = scrollValue * zoomSpeed * 0.5f; 
            targetFOV -= zoomChange;
            targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);
            vcam.Lens.FieldOfView = targetFOV;
        }
    }

    private void AssignFollowTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            vcam.Follow = player.transform;
        }
    }
}
