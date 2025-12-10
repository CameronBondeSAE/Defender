using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class NetworkedFloatingItem : NetworkBehaviour
{
   [Header("Floating Animation")]
    [SerializeField] private float floatHeight = 0.5f;
    [SerializeField] private float floatDuration = 1f;
    [SerializeField] private float rotationSpeed = 30f;
    
    private Vector3 startPosition;
    private Tween floatTween;
    private Tween rotateTween;
    
    [Header("References")]
    private Rigidbody rb;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
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
        // up n down float
        floatTween = transform.DOMoveY(startPosition.y + floatHeight, floatDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
        // lil bit of rotation
        rotateTween = transform.DORotate(new Vector3(0, 360, 0), 360f / rotationSpeed, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);
    }
    
    private void StopFloating()
    {
        floatTween?.Kill();
        rotateTween?.Kill();
    }
    
    /// <summary>
    /// when item is picked up they stop floating, basically behaves like normal prefab
    /// </summary>
    public void OnPickedUp()
    {
        StopFloating();
        if (rb != null)
        {
            rb.isKinematic = true; // inventory will handle physics
            rb.useGravity = false;
        }
        // remove this component since we're done floating
        if (IsSpawned)
        {
            RemoveFloatingBehaviorClientRpc();
        }
        Destroy(this); 
    }
    
    [ClientRpc]
    private void RemoveFloatingBehaviorClientRpc()
    {
        // Ensure floating stops on all clients
        StopFloating();
    }
    
    /// <summary>
    /// set the floating position (called by crate when spawning)
    /// </summary>
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
