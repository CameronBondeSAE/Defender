using System;
using Defender;
using Unity.Netcode;
using UnityEngine;

public class BlowingFanScript : UsableItem_Base
{
    /// Su
    // private void Start()
    // {
    //     particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    // }
    //
    // public bool fanOn;
    //
    // //Push Control
    // public void OnTriggerStay(Collider other)
    // {
    //     
    //     Rigidbody otherRigidbody = other.GetComponent<Rigidbody>();
    //     if (fanOn == true && otherRigidbody != null)
    //     {
    //         Vector3 pushDirection = (other.transform.position - transform.position).normalized;
    //         otherRigidbody.AddForce(pushDirection * pushForce, ForceMode.Acceleration);
    //     }
    // }
    //
    // //Particle Control
    // [SerializeField] private float pushForce;
    // public ParticleSystem particleSystem;
    //
    // public override void Use(CharacterBase characterTryingToUse)
    // {
    //     particleSystem.Play();
    //     fanOn = true;
    // }
    //
    // public void StopUsing()
    // {
    //     throw new System.NotImplementedException();
    // }
    //
    // public void Pickup(CharacterBase whoIsPickupMeUp)
    // {
    //     throw new System.NotImplementedException();
    // }
    //
    // public void Drop()
    // {
    //     throw new System.NotImplementedException();
    // }
    
      [Header("Push Settings")]
    [SerializeField] private float pushForce = 30f;
    [SerializeField] private LayerMask pushMask = 0;
    [Header("Visuals")]
    [SerializeField] private ParticleSystem particleSystem;
    
    [Header("Grounding")]
    [SerializeField] private float groundRaycastDistance = 2f;
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private bool snapOnSpawn = true;
    [SerializeField] private bool snapOnActivate = true;
    private bool anchorWhenOn = true;

    // NETWORKED STATE (server writes; everyone reads)
    private readonly NetworkVariable<bool> fanOnNetVar =
        new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    protected override void Awake()
    {
        base.Awake();
        if (particleSystem == null)
            particleSystem = GetComponentInChildren<ParticleSystem>(true);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // sync for late joiner
        ApplyFanVisual(fanOnNetVar.Value);
        fanOnNetVar.OnValueChanged += OnFanStateChanged;
        
        if (IsServer && snapOnSpawn && transform.parent == null)
        {
            SnapToGroundAndAlign();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        fanOnNetVar.OnValueChanged -= OnFanStateChanged;
    }

    private void OnFanStateChanged(bool previous, bool current)
    {
        ApplyFanVisual(current);
    }

    private void ApplyFanVisual(bool on)
    {
        if (particleSystem == null) return;

        if (on)
        {
            if (!particleSystem.isPlaying)
                particleSystem.Play(true);
        }
        else
        {
            if (particleSystem.isPlaying)
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
    protected override void OnManualUse(CharacterBase characterTryingToUse)
    {
        // immediate activation on button press
        TryStartActivationNow(force: true);
        ToggleFanServerRpc(true);
        SnapToGroundAndAlign();
    }

    protected override void ActivateItem()
    {
        base.ActivateItem();
        if (IsServer)
        {
            if (snapOnActivate && transform.parent == null)
            {
                SnapToGroundAndAlign();
            }
            fanOnNetVar.Value = true;
            SetFanVisualClientRpc(true); 
        }
    }

    [Rpc(SendTo.Everyone, Delivery = RpcDelivery.Reliable)]
    private void SetFanVisualClientRpc(bool on)
    {
        ApplyFanVisual(on);
    }
    
    private void SetFan(bool on)
    {
        if (!IsServer) return;
        if (on && anchorWhenOn && transform.parent == null)
        {
            // anchor if it’s placed
            SnapToGroundAndAlign();
            AnchorFanOnGround();
        }

        fanOnNetVar.Value = on;
        SetFanVisualClientRpc(on);
    }
    
    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void ToggleFanServerRpc(bool on)
    {
        fanOnNetVar.Value = on; // toggle netvar change
    }
    private void OnTriggerStay(Collider other)
    {
        // if (!IsServer) return;
        if (!fanOnNetVar.Value) return;
        Rigidbody otherRigidbody = other.attachedRigidbody;
        if (otherRigidbody == null) return;
        if (otherRigidbody.isKinematic) return;

        if (((1 << other.gameObject.layer) & pushMask) == 0) return;
        // push away from the fan's position
        Vector3 pushDirection = (otherRigidbody.worldCenterOfMass - transform.position).normalized;
        otherRigidbody.AddForce(pushDirection * pushForce, ForceMode.Acceleration);
    }
    
    private void SnapToGroundAndAlign()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        RaycastHit hit;

        if (Physics.Raycast(origin, Vector3.down, out hit, groundRaycastDistance, groundMask))
        {
            transform.position = hit.point;
            // keep Y rot, no X/Z so it's upright
            Vector3 euler = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, euler.y, 0f);
        }
    }
    
    private void AnchorFanOnGround()
    {
        if (rb == null) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    // make it moveable again, call this:
    private void ReleaseFan()
    {
        if (rb == null) return;

        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
    }
    
    public override void Use(CharacterBase characterTryingToUse)
    {
        OnManualUse(characterTryingToUse);
    }

    public override void StopUsing()
    {
        ToggleFanServerRpc(false);
        if (IsServer)
        {
            ReleaseFan();
            SetFan(false); 
        }
    }
}
