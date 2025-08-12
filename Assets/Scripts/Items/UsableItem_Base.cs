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

    [Header("Expiry Settings")]
    [Tooltip("After activation the item remains functional for this many seconds, then is destroyed.")]
    public float expiryDuration = 0f;
    // NetVars for expiry
    private NetworkVariable<float> expiryTimeRemaining = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<bool> isExpiryActive = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // local UI coroutine for expiry
    private Coroutine expiryUICoroutine;

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
    // fixed: track if the item has already been activated
    private NetworkVariable<bool> hasActivated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    protected bool isArmed = false;
    protected Coroutine activationCoroutine;
    private Coroutine activationUICoroutine;


    #region Lifecycle
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
        
        expiryTimeRemaining.OnValueChanged += OnExpiryTimeChanged;
        isExpiryActive.OnValueChanged += OnExpiryActiveChanged;

        // if we spawned while expiry already running, bring UI back JUST IN CASE
        if (isExpiryActive.Value)
        {
            StartExpiryCountdown_LocalUI(Mathf.CeilToInt(expiryTimeRemaining.Value));
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
        expiryTimeRemaining.OnValueChanged -= OnExpiryTimeChanged;
        isExpiryActive.OnValueChanged -= OnExpiryActiveChanged;
        DestroyCountdownUI();
        base.OnNetworkDespawn();
    }

    protected void OnDestroy()
    {
        DestroyCountdownUI();
    }
    #endregion

    #region NetVar Change Handlers (Activation & Expiry)
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
    
    private void OnExpiryActiveChanged(bool prev, bool nowActive)
    {
        if (nowActive)
        {
            StartExpiryCountdown_LocalUI(Mathf.CeilToInt(expiryTimeRemaining.Value));
        }
        else
        {
            if (countdownUIInstance != null)
            {
                countdownUIInstance.Hide();
            }
            if (expiryUICoroutine != null)
            {
                StopCoroutine(expiryUICoroutine);
                expiryUICoroutine = null;
            }
        }
    }

    private void OnExpiryTimeChanged(float prev, float nowVal)
    {
        if (countdownUIInstance != null && isExpiryActive.Value)
        {
            countdownUIInstance.SetCountdown(Mathf.CeilToInt(nowVal));
        }
    }

    private void OnArmedStateChanged(bool previousvalue, bool newvalue)
    {
        // isArmed = newvalue;
        // if (!newvalue)
        // {
        //     DestroyCountdownUI();
        //     if (activationCoroutine != null)
        //     {
        //         StopCoroutine(activationCoroutine);
        //         activationCoroutine = null;
        //     }
        // }
        isArmed = newvalue;

        if (!newvalue)
        {
            // Do NOT Destroy the UI here so that it doesn't race the expiry UI start.
            if (countdownUIInstance != null)
            {
                // if expiry is active (or about to be active), ensure style is correct.
                if (isExpiryActive.Value)
                {
                    countdownUIInstance.SetExpiryStyle(true);
                    countdownUIInstance.Show();
                }
                else if (!isCountdownActive.Value)
                {
                    // if no activation + no expiry just hide (still keep instance for reuse)
                    countdownUIInstance.Hide();
                }
            }

            if (activationUICoroutine != null)
            {
                StopCoroutine(activationUICoroutine);
                activationUICoroutine = null;
            }
        }
    }
    #endregion

    #region Helpers: Colliders, UI destroy, Carrier
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

    protected void SetCarrier(Transform carrier) // for ui stuff, set in subclasses
    {
        CurrentCarrier = carrier;
    }
    #endregion

    #region Interface Implementation: IPickup / IUsable
    // IPickup
    public virtual void Pickup()
    {
        if (audioSource && pickupClip) audioSource.PlayOneShot(pickupClip);
        IsCarried = true;
        // SetCarrier(CurrentCarrier);
        //CurrentCarrier = transform.parent;

        // Disable physics and colliders while in inventory
        if (rb != null)
        {
	        rb.isKinematic = true;
	        rb.useGravity  = false;
        }
        SetCollidersEnabled(false);
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
        if (hasActivated.Value || isCountdownActive.Value || isExpiryActive.Value) return;

        if (activationCountdown > 0)
        {
            if (IsServer)
            {
                StartActivationCountdown_Server();
            }
            else
            {
                // RequestDisarmServerRpc();
                RequestActivationServerRpc();
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

    public virtual void StopUsing() { /* Override in subclasses */ }
    #endregion

    #region RPCs: Client/Server (Activation entries)
    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void RequestImmediateActivationServerRpc()
    {
        if (hasActivated.Value || isCountdownActive.Value || isExpiryActive.Value) return;
        ActivateItem();
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void RequestActivationServerRpc()
    {
        if (hasActivated.Value || isCountdownActive.Value || isExpiryActive.Value) return; 
        StartActivationCountdown_Server();
    }
    #endregion

    #region Activation UI RPCs
    public virtual void StartActivationCountdown_Server()
    {
        if (!IsServer) return;
        // fixed: don't start activation countdown if already activated or in expiry
        if (hasActivated.Value || isExpiryActive.Value || isCountdownActive.Value) return;
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
        // mark as activated
        hasActivated.Value = true;
        // if have expiry, start it FIRST to avoid UI race
        bool hasExpiry = expiryDuration > 0f;
        if (hasExpiry)
        {
            StartExpiryCountdown_Server();
        }
        // clear activation NetVars
        isArmedNetworked.Value = false;
        isCountdownActive.Value = false;
        countdownTimeRemaining.Value = 0f;
        // if no expiry, keep current behavior
    }
    #endregion

    #region Expiry UI RPCs
    public void StartExpiryCountdown_Server()
    {
        if (!IsServer) return;

        isExpiryActive.Value = true;
        expiryTimeRemaining.Value = expiryDuration;

        if (activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
            activationCoroutine = null;
        }

        // kick clients to show expiry UI in expiry color
        StartExpiryUIClientRpc(Mathf.CeilToInt(expiryDuration));

        StartCoroutine(ExpiryCountdownRoutine_Server());
    }

    private IEnumerator ExpiryCountdownRoutine_Server()
    {
        float time = expiryDuration;
        while (time > 0f)
        {
            expiryTimeRemaining.Value = time;
            yield return new WaitForSeconds(1f);
            time -= 1f;
        }
        expiryTimeRemaining.Value = 0f;
        isExpiryActive.Value = false;
        // destroy on server; despawn across network
        DestroyItem_Server();
    }

    [Rpc(SendTo.Everyone, Delivery = RpcDelivery.Reliable)]
    private void StartExpiryUIClientRpc(int startSeconds)
    {
        StartExpiryCountdown_LocalUI(startSeconds);
    }

    public void StartExpiryCountdown_LocalUI(int startSeconds)
    {
        if (!countdownUIPrefab) { Debug.LogWarning($"[{name}] No countdownUIPrefab"); return; }

        if (!countdownUIInstance)
        {
            // init with expiry style
            countdownUIInstance = Instantiate(countdownUIPrefab);
            countdownUIInstance.Init(this, startSeconds, useExpiryStyle: true);
        }
        else
        {
            countdownUIInstance.Show();
            countdownUIInstance.SetExpiryStyle(true); // switch to expiry color
            countdownUIInstance.SetCountdown(startSeconds);
        }

        if (expiryUICoroutine != null) StopCoroutine(expiryUICoroutine);
        expiryUICoroutine = StartCoroutine(ExpiryCountdownRoutine_LocalUI());
    }

    private IEnumerator ExpiryCountdownRoutine_LocalUI()
    {
        while (isExpiryActive.Value && countdownUIInstance != null)
        {
            int timeLeft = Mathf.CeilToInt(expiryTimeRemaining.Value);
            countdownUIInstance.SetCountdown(timeLeft);
            if (timeLeft <= 0) break;
            yield return new WaitForSeconds(.2f);
        }

        if (countdownUIInstance != null)
        {
            countdownUIInstance.SetCountdown(0);
            countdownUIInstance.Hide();
        }
        expiryUICoroutine = null;
        yield break;
    }
    #endregion
    
    #region Destroy Item
    /// <summary>
    /// Public method to destroy/despawn this item immediately.
    /// can be called from anywhere,server-authoritative.
    /// </summary>
    public void DestroyItem()
    {
        if (IsServer)
        {
            DestroyItem_Server();
        }
        else
        {
            RequestDestroyItemServerRpc();
        }
    }

    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void RequestDestroyItemServerRpc()
    {
        DestroyItem_Server();
    }

    /// <summary>
    /// actually does the destruction on the server.
    /// </summary>
    protected virtual void DestroyItem_Server()
    {
        // clean netvars
        isArmedNetworked.Value = false;
        isCountdownActive.Value = false;
        countdownTimeRemaining.Value = 0f;
        isExpiryActive.Value = false;
        expiryTimeRemaining.Value = 0f;

        // despawn or destroy
        var no = GetComponent<NetworkObject>();
        if (no && no.IsSpawned)
        {
            no.Despawn(true); // destruction point
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Disarm (and its RPCs)
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
        
        // also cancel expiry if running!
        isExpiryActive.Value = false;
        expiryTimeRemaining.Value = 0f;
    }
    #endregion

    #region Launch / Drop (physics)
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
    #endregion
}
