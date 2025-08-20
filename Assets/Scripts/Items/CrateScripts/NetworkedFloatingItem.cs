using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class NetworkedFloatingItem : NetworkBehaviour
{
   [Header("Floating Params")]
    [SerializeField] private float floatHeight = 0.5f;
    [SerializeField] private float floatDuration = 1f;
    [SerializeField] private float rotationSpeed = 30f;
    
    private Vector3 startPosition;
    private Tween floatTween;
    private Tween rotateTween;
    
    private Rigidbody rb;
    // private Collider[] itemColliders;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        // itemColliders = GetComponentsInChildren<Collider>();
        // foreach (var collider in itemColliders)
        // {
        //     collider.enabled = false;
        // }
        
        // Start floating animation
        startPosition = transform.position;
        StartFloating();
    }
    
    public override void OnNetworkDespawn()
    {
        StopFloating();
        base.OnNetworkDespawn();
    }
    
    private void StartFloating()
    {
        // floating up n down
        floatTween = transform.DOMoveY(startPosition.y + floatHeight, floatDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        // a lil bit of rotation
        rotateTween = transform.DORotate(new Vector3(0, 360, 0), 360f / rotationSpeed, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);
    }
    
    private void StopFloating()
    {
        floatTween?.Kill();
        rotateTween?.Kill();
    }
    public void OnPickedUp()
    {
        StopFloating();
        // Re-enable colliders for pickup
        // foreach (var collider in itemColliders)
        // {
        //     collider.enabled = true;
        // }
        
        // physics is by the pickup system
    }
    public void SetFloatingPosition(Vector3 position)
    {
        startPosition = position;
        transform.position = position;
        if (floatTween != null)
        {
            StopFloating();
            StartFloating();
        }
    }
}
