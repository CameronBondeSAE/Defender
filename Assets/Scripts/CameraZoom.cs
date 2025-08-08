using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraZoom : MonoBehaviour
{
    public string playerTag = "Player";
    public bool updateContinuously = false;
    public CinemachineCamera vcam;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minSize = 20f;
    [SerializeField] private float maxSize = 80f;
    private float targetSize;


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
        float scrollValue = Mouse.current.scroll.ReadValue().y;
        if (scrollValue != 0)
        {
            targetSize -= scrollValue * zoomSpeed * Time.deltaTime;
            targetSize = Mathf.Clamp(targetSize, minSize, maxSize);
            vcam.Lens.OrthographicSize = targetSize;
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
