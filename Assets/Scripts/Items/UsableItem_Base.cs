using System.Collections;
using System.Collections.Generic;
using Defender;
using Unity.Burst.Intrinsics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

/// <summary>
/// Base for any inventory item you can use, arm (start a countdown), or launch (throw).
/// Inherit and override Use, OnArmed, Explode, etc for your specific item logic.
/// </summary>
public class UsableItem_Base : NetworkBehaviour, IPickup, IUsable
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pickupClip, dropClip, useClip, timerBeepClip, timerActivatedClip;

    [Header("Item Settings")]
    [SerializeField] private bool isConsumable = true;
    public virtual bool IsConsumable => isConsumable;

    [Header("Activation Settings")]
    [Tooltip("If > 0, activates countdown before item is activated")]
    public float activationCountdown = 0f;

    [Header("World UI - Assign in prefab!")]
    [SerializeField] private CountdownUI countdownUIPrefab; // drag the child UI here:D
    // prefab instance (not parented to the item!)
    private CountdownUI countdownUIInstance;
    // tracking settings
    public bool      IsCarried      { get; private set; }
    public Transform CurrentCarrier { get; set; }

    [Header("Launch Settings")]
    public float launchForce = 10f;
    [SerializeField] protected Vector3 launchDirection = Vector3.forward;
    protected Rigidbody rb;
    
    [Header("NetworkVar")]
    private NetworkVariable<bool> isArmedNetworked = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> countdownTimeRemaining = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> isCountdownActive = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    protected bool isArmed = false;
    protected Coroutine activationCoroutine;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
	        rb.isKinematic = true;
	        rb.useGravity  = false;
        }
        // disable all colliders while held in inventory (start as pickupable)
        SetCollidersEnabled(true);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isArmedNetworked.OnValueChanged += OnArmedStateChanged;
        countdownTimeRemaining.OnValueChanged += OnCountdownTimeChanged;
        isCountdownActive.OnValueChanged += OnCountdownActiveChanged;
        // init ui in case its armed when we spawn
        if (isArmedNetworked.Value && isCountdownActive.Value)
        {
            StartActivationCountdown_LocalUI(Mathf.CeilToInt(countdownTimeRemaining.Value));
        }
    }

    public override void OnNetworkDespawn()
    {
        if (isArmedNetworked != null)
        {
            isArmedNetworked.OnValueChanged -= OnArmedStateChanged;
            countdownTimeRemaining.OnValueChanged -= OnCountdownTimeChanged;
            isCountdownActive.OnValueChanged -= OnCountdownActiveChanged;
        }
        DestroyCountdownUI();
        base.OnNetworkDespawn();
    }

    private void OnCountdownActiveChanged(bool previousvalue, bool newvalue)
    {
        if (newvalue && isArmedNetworked.Value)
        {
            StartActivationCountdown_LocalUI(Mathf.CeilToInt(countdownTimeRemaining.Value));
        }
        else
        {
            if (countdownUIInstance != null)
            {
                countdownUIInstance.Hide();
            }

            if (activationUICoroutine != null)
            {
                StopCoroutine(activationUICoroutine);
                activationUICoroutine = null;
            }
        }
    }

    private void OnCountdownTimeChanged(float previousvalue, float newvalue)
    {
        if (countdownUIInstance != null && isCountdownActive.Value)
        {
            countdownUIInstance.SetCountdown(Mathf.CeilToInt(newvalue));
        }
    }

    private void OnArmedStateChanged(bool previousvalue, bool newvalue)
    {
        isArmed = newvalue;
        if (!newvalue)
        {
            DestroyCountdownUI();
            if (activationCoroutine != null)
            {
                StopCoroutine(activationCoroutine);
                activationCoroutine = null;
            }
        }
    }

    protected void OnDestroy()
    {
        DestroyCountdownUI();
    }

    protected void DestroyCountdownUI()
    {
        if (countdownUIInstance)
        {
            Destroy(countdownUIInstance.gameObject);
            countdownUIInstance = null;
        }
    }

    // helper - enable or disable all colliders
    protected void SetCollidersEnabled(bool enabled)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (var col in colliders)
            col.enabled = enabled;
    }

    // IPickup
    public virtual void Pickup()
    {
        if (audioSource && pickupClip) audioSource.PlayOneShot(pickupClip);
        IsCarried = true;
        SetCarrier(CurrentCarrier);
        //CurrentCarrier = transform.parent;

        // Disable physics and colliders while in inventory
        if (rb != null)
        {
	        rb.isKinematic = true;
	        rb.useGravity  = false;
        }
        SetCollidersEnabled(false);
    }
    
    protected void SetCarrier(Transform carrier) // for ui stuff, set in subclasses
    {
        CurrentCarrier = carrier;
    }

    public virtual void Drop()
    {
        IsCarried = false;
        CurrentCarrier = null;
        transform.SetParent(null, true);

        // re-enable colliders and physics
        SetCollidersEnabled(true);
        if (rb != null)
        {
	        rb.isKinematic            = false;
	        rb.useGravity             = true;
	        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        if (audioSource && dropClip) audioSource.PlayOneShot(dropClip);
    }

    // IUsable
    public virtual void Use(CharacterBase characterTryingToUse)
    {
        if (audioSource && useClip) audioSource.PlayOneShot(useClip);

        if (activationCountdown > 0)
        {
            if (IsServer)
            {
                StartActivationCountdown_Server();
            }
            else
            {
                RequestDisarmServerRpc();
            }
        }
        else
        {
            if (IsServer)
            {
                ActivateItem();
            }
            else
            {
                RequestImmediateActivationServerRpc();
            }
        }
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void RequestImmediateActivationServerRpc()
    {
        ActivateItem();
    }

    public virtual void StopUsing() { /* Override in subclasses */ }
    

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void RequestActivationServerRpc()
    {
        StartActivationCountdown_Server();
    }
    
    public virtual void StartActivationCountdown_Server()
    {
        if (!IsServer) return;
        // set netvar state
        isArmedNetworked.Value = true;
        countdownTimeRemaining.Value = activationCountdown;
        isCountdownActive.Value = activationCountdown > 0;
        if (activationCountdown > 0)
        {
            if (activationCoroutine != null) StopCoroutine(activationCoroutine);
            activationCoroutine = StartCoroutine(ActivationCountdownRoutine_Server());
            // notify client to start ui
            StartCountdownUIClientRpc(Mathf.CeilToInt(activationCountdown));
        }
        else
        {
            ActivateItem();
        }
    }
    protected virtual IEnumerator ActivationCountdownRoutine_Server()
    {
        float time = activationCountdown;
        while (time > 0f)
        {
            countdownTimeRemaining.Value = time;
            PlayTimerBeepClientRpc(); // play cound down beep on all clients
            yield return new WaitForSeconds(1f);
            time -= 1f;
        }
        countdownTimeRemaining.Value = 0f;
        isCountdownActive.Value = false;
        PlayTimerActivatedClientRpc(); // play a sound on all clients upon timer activation
        ActivateItem(); 
    }
    
    [Rpc(SendTo.Everyone, Delivery = RpcDelivery.Reliable)]
    private void StartCountdownUIClientRpc(int startSeconds)
    {
        StartActivationCountdown_LocalUI(startSeconds);
    }

    [Rpc(SendTo.Everyone, Delivery = RpcDelivery.Reliable)]
    private void PlayTimerActivatedClientRpc()
    {
        if(audioSource && timerActivatedClip) audioSource.PlayOneShot(timerActivatedClip);
    }

    [Rpc(SendTo.Everyone, Delivery = RpcDelivery.Unreliable)] //unreliable for frequent beep
    private void PlayTimerBeepClientRpc()
    {
        if(audioSource && timerBeepClip) audioSource.PlayOneShot(timerBeepClip);
    }

    public void StartActivationCountdown_LocalUI(int startSeconds)
    {
        if (!countdownUIPrefab) { Debug.LogWarning($"[{name}] No countdownUIPrefab"); return; }

        if (!countdownUIInstance)
        {
            countdownUIInstance = Instantiate(countdownUIPrefab);
            countdownUIInstance.Init(this, startSeconds);
        }
        else
        {
            countdownUIInstance.Show();
            countdownUIInstance.SetCountdown(startSeconds);
        }
        if (activationUICoroutine != null) StopCoroutine(activationUICoroutine);
        activationUICoroutine = StartCoroutine(ActivationCountdownRoutine_LocalUI());
    }

    private Coroutine activationUICoroutine;

    private IEnumerator ActivationCountdownRoutine_LocalUI()
    {
        while (isCountdownActive.Value && countdownUIInstance != null)
        {
            int timeLeft = Mathf.CeilToInt(countdownTimeRemaining.Value);
            countdownUIInstance.SetCountdown(timeLeft);
            if (timeLeft <= 0) break;
            yield return new WaitForSeconds(.2f); // updating the ui slightly more frequently than server
        }

        if (countdownUIInstance != null)
        {
            countdownUIInstance?.SetCountdown(0);
            countdownUIInstance?.Hide();
        }
        activationUICoroutine = null;
    }

    protected virtual void ActivateItem()
    {
        Debug.Log($"{gameObject.name} activated!");
        if (!IsServer) return;
        // resetting netvar states
        isArmedNetworked.Value = false;
        isCountdownActive.Value = false;
        countdownTimeRemaining.Value = 0f;
    }

    public virtual void Disarm()
    {
        if (IsServer)
        {
            DisarmServerRpc();
        }
        else
        {
            RequestDisarmServerRpc();
        }
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void RequestDisarmServerRpc()
    {
        DisarmServerRpc();
    }
    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void DisarmServerRpc()
    {
        if (activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
            activationCoroutine = null;
        }
        // clean netvars
        isArmedNetworked.Value = false;
        isCountdownActive.Value = false;
        countdownTimeRemaining.Value = 0f;
    }

    // Launch/drop (NOTE: don't call Drop() inside Drop(Vector3))
    public virtual void Launch(Vector3 direction, float force)
    {
        SetCollidersEnabled(true);
        
        transform.parent = null;
        
        if (rb != null)
        {
	        rb.isKinematic            = false;
	        rb.useGravity             = true;
	        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

	        rb.linearVelocity  = Vector3.zero;
	        rb.angularVelocity = Vector3.zero;

	        rb.AddForce(direction * force, ForceMode.VelocityChange);
        }
    }

    public virtual void Drop(Vector3 dropPosition)
    {
        // re-enable physics and colliders
        SetCollidersEnabled(true);
        if (rb != null)
        {
	        rb.isKinematic            = false;
	        rb.useGravity             = true;
	        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

	        rb.linearVelocity  = Vector3.zero;
	        rb.angularVelocity = Vector3.zero;
        }

        transform.position = dropPosition;

        if (audioSource && dropClip) audioSource.PlayOneShot(dropClip);

        IsCarried = false;
        CurrentCarrier = null;
        // Don't call Disarm(); in Drop(Vector3) because item may still be armed after throw/drop!
    }
}
