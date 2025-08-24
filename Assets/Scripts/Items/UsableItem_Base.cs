using System;
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
/// base class for any "useable" item that lives on the network. 
/// tl;dr: the server is the single source of truth (it owns the timers, state, and despawn),
/// and clients mainly show ui and play sounds based on the server's network variables (netvars).
public class UsableItem_Base : NetworkBehaviour, IPickup, IUsable, IDescribable
{
       [Header("Audio")]
    [Tooltip("audio source for all sfx (pickup, drop, use, beep, activated). null = mute.")]
    public AudioSource audioSource;

    // note: unity only applies a [Tooltip] to the first field when multiple are declared on one line.
    // keeping your format; the tooltip below documents the whole group.
    [Tooltip("pickup: Pickup(), drop: Drop(), use: Use(), beep: per-second during activation, activated: when countdown hits 0.")]
    public AudioClip pickupClip, dropClip, useClip, timerBeepClip, timerActivatedClip;

    [Header("Item Settings")]
    [SerializeField] 
    [Tooltip("single-use hint for gameplay. base doesn't auto-destroy; use expiryDuration or DestroyItem().")]
    private bool isConsumable = true;
    public virtual bool IsConsumable => isConsumable;

    [Header("Expiry Settings")]
    [Tooltip("seconds active after activation, then auto-despawn. 0 = no auto-despawn; you destroy it.")]
    public float expiryDuration = 0f;
    // netvar: how many seconds of expiry are left
    private NetworkVariable<float> expiryTimeRemaining = new NetworkVariable<float>(
        0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // netvar: are we currently in the expiry phase (post-activation)?
    private NetworkVariable<bool> isExpiryActive = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // local UI coroutine for expiry
    private Coroutine expiryUICoroutine;
    private bool _awaitingExpiryUiStart; // to fix out of order netvar sync

    [Header("Activation Settings")]
    [Tooltip("delay before activation. 0 = immediate. OnUse: Use() starts it. Manual: start via code.")]
    public float activationCountdown = 0f;

    public enum ActivationTriggerMode { OnUse, Manual }

    [SerializeField] 
    [Tooltip("OnUse: Use() starts activation. Manual: call TryStartActivation(...) / StartActivationCountdown_Server(...).")]
    private ActivationTriggerMode activationTrigger = ActivationTriggerMode.OnUse;

    [SerializeField] 
    [Tooltip("allow re-arming after activation. false = one-shot; true = reusable (you control when).")]
    private bool allowMultipleActivations = false;

    [Header("Read-only flags for Debugging during runtime")]
    [Tooltip("True after ActivateItem() runs at least once.")]
    public bool IsActivated      => hasActivated.Value;

    [Tooltip("True during server countdown (Activation). Driven by NetVar isCountdownActive.")]
    public bool IsCountdownActive => isCountdownActive.Value;

    [Tooltip("True during server expiry (post-activation). Driven by NetVar isExpiryActive.")]
    public bool IsExpiryActive    => isExpiryActive.Value;

    [Header("Countdown UI - Assign in prefab!")]
    [SerializeField] 
    [Tooltip("world-space countdown UI used for activation and expiry.")]
    private CountdownUI countdownUIPrefab; // drag the child UI here:D
    // prefab instance (not parented to the item!)
    private CountdownUI countdownUIInstance;
    
    [Header("Item Description")]
    [SerializeField] private string itemName;
    [SerializeField] private string description;
    public string ItemName => itemName;
    public string Description => description;

    // tracking settings
    [Tooltip("set in Pickup()/Drop(). debug only.")]
    public bool      IsCarried      { get; private set; }

    // property is not serialized; no tooltip needed
    public Transform CurrentCarrier { get; set; }

    [Header("Launch Settings")]
    [Tooltip("magnitude of the throw impulse used by Launch(). applied as ForceMode.VelocityChange on the server. requires a Rigidbody; if none, Launch() will do nothing.\nuse for throwables/physics-based deployables.")]
    public float launchForce = 10f;

    [SerializeField] 
    [Tooltip("optional default direction for derived classes that call Launch(transform.TransformDirection(launchDirection), force). the base Launch(direction, force) ignores this field unless your subclass uses it explicitly.")]
    protected Vector3 launchDirection = Vector3.forward;

    protected Rigidbody rb;

    [Header("NetworkVar")]
    // netvar: is the item "armed" on the network?
    private NetworkVariable<bool> isArmedNetworked = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // netvar: seconds left in the activation countdown (pre-activation).
    private NetworkVariable<float> countdownTimeRemaining = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // netvar: are we currently counting down to activation?
    private NetworkVariable<bool> isCountdownActive = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    // fixed: track if the item has already been activated
    private NetworkVariable<bool> hasActivated = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    protected bool isArmed = false;
    protected Coroutine activationCoroutine;
    private Coroutine activationUICoroutine;


    #region Lifecycle
    protected virtual void Awake()
    {
        // standard hooks, initialized the item to be rb active to begin with
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
	        rb.isKinematic = true;
	        rb.useGravity  = false;
        }
        // disable all colliders while held in inventory (start as pickupable)
        SetCollidersEnabled(true);
    }

    /// <summary>
    /// this is where netvars are hooked up
    /// server and clients both subscribe to OnValueChanged so they react when the server updates state.
    /// for late-joiners: if a client spawns after everything's already set, we can manually nudge the handlers
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isArmedNetworked.OnValueChanged += OnArmedStateChanged;
        countdownTimeRemaining.OnValueChanged += OnCountdownTimeChanged;
        isCountdownActive.OnValueChanged += OnCountdownActiveChanged;
        expiryTimeRemaining.OnValueChanged += OnExpiryTimeChanged;
        isExpiryActive.OnValueChanged += OnExpiryActiveChanged;
        
        if (!IsServer) // Only needed on clients
        {
            // Check if armed and invoke handler if needed
            if (isArmedNetworked.Value)
            {
                Debug.Log($"Client late-join: Manually invoking OnArmedStateChanged");
                OnArmedStateChanged(false, isArmedNetworked.Value);
            }
        
            // Check if countdown is active and invoke handler if needed
            if (isCountdownActive.Value)
            {
                Debug.Log($"Client late-join: Manually invoking OnCountdownActiveChanged");
                OnCountdownActiveChanged(false, isCountdownActive.Value);
            }
        
            // Check if expiry is active and invoke handler if needed
            if (isExpiryActive.Value)
            {
                Debug.Log($"Client late-join: Manually invoking OnExpiryActiveChanged");
                OnExpiryActiveChanged(false, isExpiryActive.Value);
            }
        }

        // init ui in case its armed when we spawn
        if (isArmedNetworked.Value && isCountdownActive.Value)
        {
            StartActivationCountdown_LocalUI(Mathf.CeilToInt(countdownTimeRemaining.Value));
        }
        // if we spawned while expiry already running, bring UI back JUST IN CASE
        if (isExpiryActive.Value)
        {
            StartExpiryCountdown_LocalUI(Mathf.CeilToInt(expiryTimeRemaining.Value));
        }
        
        Debug.Log($"OnNetworkSpawn on {(IsServer ? "Server" : "Client")}: " +
                  $"isCountdownActive={isCountdownActive.Value}, " +
                  $"isExpiryActive={isExpiryActive.Value}, " +
                  $"expiryTimeRemaining={expiryTimeRemaining.Value}");
    }

    /// <summary>
    /// cleanups: unhook netvar listeners on despawn and kill any spawned ui.
    /// server and clients both do this locally
    /// </summary>
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
        Debug.Log($"OnNetworkDespawn on {(IsServer ? "Server" : "Client")} NO={NetworkObject?.NetworkObjectId}");
        base.OnNetworkDespawn();
    }

    protected void OnDestroy()
    {
        DestroyCountdownUI();
    }
    #endregion

    #region NetVar Change Handlers (Activation & Expiry)
    /// <summary>
    /// fires on both server and clients whenever the server toggles countdown active,
    /// so THIS is where clients spawn/show/hide the activation countdown ui.
    /// </summary>
    private void OnCountdownActiveChanged(bool previousvalue, bool newvalue)
    {
        Debug.Log($"OnCountdownActiveChanged: {previousvalue} -> {newvalue} on {(IsServer ? "Server" : "Client")}");
        if (newvalue && isArmedNetworked.Value)
        {
            // ceil converts a float like 2.3 to 3 so the ui shows whole seconds remaining, but internally it still tracks the exact value:)
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

    /// <summary>
    /// called whenever the server updates the countdown seconds
    /// clients use this to refresh the on-screen number
    /// </summary>
    private void OnCountdownTimeChanged(float previousvalue, float newvalue)
    {
        if (countdownUIInstance != null && isCountdownActive.Value)
        {
            countdownUIInstance.SetCountdown(Mathf.CeilToInt(newvalue));
        }
    }
    
    /// <summary>
    /// toggles expiry ui when server flips expiry state.
    /// handles the "ordering" problem by waiting for a non-zero time to arrive before starting ui.
    /// </summary>
    private void OnExpiryActiveChanged(bool prev, bool nowActive)
    {
        if (nowActive)
        {
            int start = Mathf.CeilToInt(expiryTimeRemaining.Value);
            if (start > 0)
            {
                StartExpiryCountdown_LocalUI(start);
                _awaitingExpiryUiStart = false;
            }
            else
            {
                // time hasn't arrived yet; wait for OnExpiryTimeChanged to kick UI off
                _awaitingExpiryUiStart = true;
            }
        }
        else
        {
            _awaitingExpiryUiStart = false;

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

    /// <summary>
    /// live updates for expiry time left, driven on the server
    /// if we are about to get/waiting for a first non-zero tick, this function kicks the ui off here
    private void OnExpiryTimeChanged(float prev, float nowVal)
    {
        if (!isExpiryActive.Value) return;

        int now = Mathf.CeilToInt(nowVal);

        // on the first non-zero tick, start or restart the UI HERE
        if (_awaitingExpiryUiStart && now > 0)
        {
            StartExpiryCountdown_LocalUI(now);
            _awaitingExpiryUiStart = false;
            return;
        }
        // normal live update path
        if (countdownUIInstance != null)
        {
            countdownUIInstance.SetCountdown(now);
            countdownUIInstance.Show(); 
        }
    }

    /// <summary>
    /// syncs the local, non-networked isArmed flag and adjusts the ui depending on whether we're in expiry or not
    /// </summary>
    private void OnArmedStateChanged(bool previousvalue, bool newvalue)
    {
        Debug.Log($"OnArmedStateChanged: {previousvalue} -> {newvalue} on {(IsServer ? "Server" : "Client")}");
    
        isArmed = newvalue;

        if (!newvalue)
        {
            // NOT destroying the UI here so that it won't race the expiry UI start
            if (countdownUIInstance != null)
            {
                if (isExpiryActive.Value)
                {
                    Debug.Log($"Armed->Disarmed but expiry active, switching UI to expiry style on {(IsServer ? "Server" : "Client")}");
                    countdownUIInstance.SetExpiryStyle(true);
                    countdownUIInstance.Show();
                }
                else if (!isCountdownActive.Value)
                {
                    Debug.Log($"Armed->Disarmed, no expiry/countdown, hiding UI on {(IsServer ? "Server" : "Client")}");
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

    #region Helpers: Colliders, UI destroy, Carrier, Description
    /// <summary>
    /// local helper: nuke the spawned countdown ui instance if it exists
    /// </summary>
    protected void DestroyCountdownUI()
    {
        if (countdownUIInstance)
        {
            Destroy(countdownUIInstance.gameObject);
            countdownUIInstance = null;
        }
    }

    // also local helper - enable or disable all colliders
    protected void SetCollidersEnabled(bool enabled)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (var col in colliders)
            col.enabled = enabled;
    }

    /// <summary>
    /// local setter to who holds the item; networking doesn't care about this directly
    /// subclasses can hook ui here
    /// </summary>
    protected void SetCarrier(Transform carrier) // for ui stuff, set in subclasses
    {
        CurrentCarrier = carrier;
    }
    
    #endregion

    #region Interface Implementation: IPickup / IUsable
    // IPickup
    /// <summary>
    /// called when the player picks up the item. plays a sound locally and tells others via a client rpc (server triggers it)
    /// also disables physics so it behaves like an inventory object.
    /// on server: plays sfx locally and also calls the PlayPickupAudioClientRpc() so every client hears it
    /// on client: just plays the local sfx; the rpc also ensures other clients hear it too
    public virtual void Pickup(CharacterBase whoIsPickupMeUp)
    {
        // synced audio
        if (audioSource && pickupClip) 
        {
            audioSource.PlayOneShot(pickupClip);
            if (IsServer)
                PlayPickupAudioClientRpc();
        }
        IsCarried = true;
        
		SetCarrier(whoIsPickupMeUp.transform);
        // Disable physics and colliders while in inventory
        if (rb != null)
        {
	        rb.isKinematic = true;
	        rb.useGravity  = false;
        }
        SetCollidersEnabled(false);
    }
    
    /// <summary>
    /// client-rpc for pickup sfx. this runs on all clients when the server calls it
    /// </summary>
    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    private void PlayPickupAudioClientRpc()
    {
        if (!IsServer && audioSource && pickupClip)
        {
            audioSource.PlayOneShot(pickupClip);
        }
    }

    /// <summary>
    /// dropping the item: physics re-enabled, colliders back on, sound plays, and server have to mirror that to others with a client rpc
    /// </summary>
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
        if (audioSource && dropClip)
        {
            audioSource.PlayOneShot(dropClip);
            if (IsServer)
                PlayDropAudioClientRpc();
        }
    }
    
    /// <summary>
    /// client-rpc for drop sfx. server calls, clients play.
    /// </summary>
    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    private void PlayDropAudioClientRpc()
    {
        if (!IsServer && audioSource && dropClip)
        {
            audioSource.PlayOneShot(dropClip);
        }
    }

    /// <summary>
    /// main "use" function from the interface that's called by player input
    /// sfx is handled locally and synced via rpc; network state changes are server-authoritative:
    /// so, I implemented immediate use and manual use:
    /// if it's OnUse mode, server either starts a countdown or activates immediately
    /// clients never directly flip the netvars; they request via a server rpc
    /// </summary>
    public virtual void Use(CharacterBase characterTryingToUse)
    {
        if (audioSource && useClip)
        {
            audioSource.PlayOneShot(useClip);
            if (IsServer)
                PlayUseAudioClientRpc();
        }

        // if already going/expired/used, ignore
        if (hasActivated.Value && !allowMultipleActivations) return;
        if (isCountdownActive.Value || isExpiryActive.Value) return;

        if (activationTrigger == ActivationTriggerMode.OnUse)
        {
            // preserve current behavior: start countdown or activate immediately
            if (IsServer)
            {
                StartActivationCountdown_Server(activationCountdown);
            }
            else
            {
                RequestTryStartActivationServerRpc(activationCountdown, false);
            }
        }
        else
        {
            // Manual mode: DO NOT start countdown/activation here.
            // subclasses can override and still call base.Use(...) to get sfx n stuff
            // here's a hook for subclasses if needed:D
            OnManualUse(characterTryingToUse);
        }
    }
    
    // networked sfx playing, self-explanatory :D
    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    private void PlayUseAudioClientRpc()
    {
        if (!IsServer && audioSource && useClip)
        {
            audioSource.PlayOneShot(useClip);
        }
    }

    public virtual void StopUsing() { /* Override in subclasses if needed */ }
    #endregion

    #region RPCs: Client/Server (Activation entries)
    
    protected virtual void OnManualUse(CharacterBase characterTryingToUse) { }  // hook for subclasses when trigger mode is manual.
    
    /// <summary>
    /// public helper you can call from anywhere to try starting activation.
    /// server: runs immediately and returns true/false based on state.
    /// client: sends a server rpc and returns true right away (meaning "request sent", not "activation guaranteed").
    /// </summary>
    public bool TryStartActivation(float? seconds = null, bool force = false)
    {
        float duration = Mathf.Max(0f, seconds ?? activationCountdown); // clamp/no negatives

        if (IsServer)
        {
            return TryStartActivation_Server(duration, force);
        }
        else
        {
            RequestTryStartActivationServerRpc(duration, force);// client asks the boss (server)
            return true; // request sent
        }
    }

    /// <summary>
    /// convenience helper to start activation immediately (0 seconds) if allowed.
    /// </summary>
    public bool TryStartActivationNow(bool force = false) => TryStartActivation(0f, force);
    
    /// <summary>
    /// server-only logic that checks gating (already used? already running?) and either starts the countdown or bails
    /// </summary>
    
    private bool TryStartActivation_Server(float seconds, bool force)
    {
        if (!force)
        {
            if ((hasActivated.Value && !allowMultipleActivations) || isCountdownActive.Value || isExpiryActive.Value)
                return false;
        }

        StartActivationCountdown_Server(seconds);
        return true;
    }
    
    /// <summary>
    /// server rpc entry from clients for starting activation
    /// server decides; again clients never changes netvars directly
    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void RequestTryStartActivationServerRpc(float seconds, bool force)
    {
        TryStartActivation_Server(seconds, force);
    }
    
    #endregion

    #region Activation UI RPCs

    /// <summary>
    /// server-side function to actually initialize the activation sequence
    /// again, only the server writes to activation netvars (armed, countdown time, isCountdownActive)
    /// when a countdown exists/start, the server will start a coroutine that'll tick per second
    /// and ticks clients with beeps + ui starts. if seconds is 0, server activates immediately
    /// </summary>
    public void StartActivationCountdown_Server(float seconds)
    {
        if (!IsServer) return;
        // don't start activation countdown if already activated or in expiry
        if (hasActivated.Value || isExpiryActive.Value || isCountdownActive.Value) return;
        // set netvar state using the provided seconds
        isArmedNetworked.Value = true;
        countdownTimeRemaining.Value = Mathf.Max(0f, seconds);
        isCountdownActive.Value = countdownTimeRemaining.Value > 0f;
        if (isCountdownActive.Value)
        {
            if (activationCoroutine != null) StopCoroutine(activationCoroutine);
            activationCoroutine = StartCoroutine(ActivationCountdownRoutine_Server_Internal(countdownTimeRemaining.Value));
            // notify client to start ui
            StartCountdownUIClientRpc(Mathf.CeilToInt(countdownTimeRemaining.Value));
        }
        else
        {
            ActivateItem();
        }
    }

    /// <summary>
    /// this function is the server's actual ticking loop for activation. writes time every second to the netvar so clients update their ui
    /// when it hits zero, the server flips state and calls ActivateItem().
    /// </summary>
    // fix: internal routine that runs with an explicit duration (manual starts/overrides can now be used:D)
    private IEnumerator ActivationCountdownRoutine_Server_Internal(float seconds)
    {
        float time = Mathf.Max(0f, seconds);
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

    /// <summary>
    /// client-rpc to kick off activation countdown ui everywhere. Rpc.reliable because it's a one-off/HAS to work
    /// </summary>
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

    /// <summary>
    /// client-rpc for the periodic beep. marked unreliable on purpose because it's frequent/happens every second
    /// and it's fine if one beep is dropped
    /// </summary>
    [Rpc(SendTo.Everyone, Delivery = RpcDelivery.Unreliable)] 
    private void PlayTimerBeepClientRpc()
    {
        if(audioSource && timerBeepClip) audioSource.PlayOneShot(timerBeepClip);
    }

    /// <summary>
    /// local (not-networked) ui starter for activation countdown.
    /// if an ui instance doesn't exist yet we spawn it, otherwise we just update/show it
    /// also starts a local coroutine to change the netvar more frequently for smoother ui - something i added as a later fix
    /// </summary>
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

    /// <summary>
    /// client-side ui loop that reads the replicated netvars and sets the number
    /// this is updated a bit faster than the server tick (.2s) so that ui feels responsive
    /// </summary>
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

    /// <summary>
    /// this is server-ONLY: flips into "activated" state and starts expiry (if it's set to start immediately)
    /// order matters here: if we need expiry, we start that first so ui has time values to read,
    /// then we clear activation netvars; all writes happen on the server and gets sent out/synced
    /// </summary>
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
    /// <summary>
    /// server function to start the expiry period. the time is set FIRST,
    /// then the active flag is set second — this fixed the clients ordering issues when turning off
    /// activation ui and starting the expiry one
    /// </summary>
    public void StartExpiryCountdown_Server()
    {
        if (!IsServer) return;
        // set the time first so clients can read a non-zero value
        expiryTimeRemaining.Value = expiryDuration;
        // THEN mark active cuz ordering across different netvars isn't freaking guaranteed
        isExpiryActive.Value = expiryDuration > 0f;
        if (activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
            activationCoroutine = null;
        }
        if (isExpiryActive.Value)
        {
            StartCoroutine(ExpiryCountdownRoutine_Server());
        }
    }

    /// <summary>
    /// server ticking loop for the expiry timer. when it ends, the server despawns the object
    /// </summary>
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

    // [Rpc(SendTo.Everyone, Delivery = RpcDelivery.Reliable)]
    // private void StartExpiryUIClientRpc(int startSeconds)
    // {
    //     Debug.Log($"StartExpiryUIClientRpc called with {startSeconds} seconds on {(IsServer ? "Server" : "Client")}");
    //     StartExpiryCountdown_LocalUI(startSeconds);
    // }

    /// <summary>
    /// local (non-networked) starter for expiry ui, switches the ui style to expiry mode and starts a local timer
    /// </summary>
    public void StartExpiryCountdown_LocalUI(int startSeconds)
    {
        Debug.Log($"StartExpiryCountdown_LocalUI called");
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

    /// <summary>
    /// client-side ui coroutine for expiry, reads the replicated netvars and ticks the number until time runs out
    /// </summary>
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
    /// public/glocal function to destroy/despawn the item. server does it directly; clients request via server rpc
    /// this keeps destruction authoritative and prevents desync/hacks
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

    /// <summary>
    /// server rpc from clients to request destruction. the server do the despawn
    /// </summary>
    [Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable)]
    private void RequestDestroyItemServerRpc()
    {
        DestroyItem_Server();
    }

    /// <summary>
    /// server-only: stops any running coroutines, clears netvars, and despawns the network object.
    /// if the object isn't spawned, log an error because clients won't see a despawn otherwise (was used when i was debugging).
    /// </summary>
    protected virtual void DestroyItem_Server()
    {
        if (!IsServer) return;
        if (activationCoroutine != null) { StopCoroutine(activationCoroutine); activationCoroutine = null; }
        if (expiryUICoroutine != null)   { StopCoroutine(expiryUICoroutine);   expiryUICoroutine   = null; }
        Debug.Log($"DestroyItem_Server Destroying {gameObject.name}");
        ClearFromInventoryBeforeDestroy();
        // clean netvars
        isArmedNetworked.Value = false;
        isCountdownActive.Value = false;
        countdownTimeRemaining.Value = 0f;
        isExpiryActive.Value = false;
        expiryTimeRemaining.Value = 0f;
        // despawn or destroy
        var no = NetworkObject;
        if (no && no.IsSpawned)
        {
            Debug.Log($"[DestroyItem_Server] Despawning NO={no.NetworkObjectId} (pool-friendly).");
            no.Despawn(true); // destruction point
        }
        else
        {
            Debug.LogError($"[DestroyItem_Server] NetworkObject null or !IsSpawned. Clients will NOT despawn!");
        }
    }

    /// <summary>
    /// local helper function - if someone's inventory currently references this exact item instance, clear it
    /// before we despawn so their ui/state doesn't dangle, wrote this when fixing/syncing inventory scripts
    /// </summary>
    private void ClearFromInventoryBeforeDestroy()
    {
        if (CurrentCarrier != null)
        {
            PlayerInventory inventory = CurrentCarrier.GetComponent<PlayerInventory>();
            if (inventory != null && inventory.CurrentItemInstance == this.gameObject)
            {
                Debug.Log($"Clearing {gameObject.name} from {CurrentCarrier.name}'s inventory");
                inventory.ClearCurrentItemWithoutDestroy();
            }
        }
    }

    #endregion

    #region Disarm (and its RPCs)
    /// <summary>
    /// public function to cancel any active countdown/expiry and clear netvars
    /// again, server does directly; clients request via server rpc
    /// </summary>
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

    /// <summary>
    /// server: kills activation coroutine (if active) and resets all activation/expiry netvars
    /// clients will react via OnValueChanged and hide their ui
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
    
    /// <summary>
    /// throws the item using physics. drop is called first to release it, then we add a velocity-change impulse
    /// this base method doesn't directly do that tho, to use/access it: call from server or sync via rpcs.
    /// </summary>
    public virtual void Launch(Vector3 direction, float force)
    {
        Drop();
        SetCollidersEnabled(true);
        // transform.parent = null;
        if (rb == null) rb = GetComponent<Rigidbody>();

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

    /// <summary>
    /// drops the item at an explicit dropPosition, re-enabling physics locally and playing the sfx locally
    /// </summary>
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
