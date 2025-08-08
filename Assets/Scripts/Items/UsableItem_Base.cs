using System.Collections;
using System.Collections.Generic;
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

    [Header("Launch Settings")]
    public float launchForce = 10f;
    [SerializeField] protected Vector3 launchDirection = Vector3.forward;
    protected Rigidbody rb;

    protected bool isArmed = false;
    protected Coroutine activationCoroutine;

    // track carrier/player
    public bool      IsCarried      { get; private set; }
    public Transform CurrentCarrier { get; set; }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        if (rb != null)
        {
	        rb.isKinematic = true;
	        rb.useGravity  = false;
        }
        // disable all colliders while held in inventory (start as pickupable)
        SetCollidersEnabled(true);
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
        // CurrentCarrier = transform.parent;

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
    public virtual void Use()
    {
        if (audioSource && useClip) audioSource.PlayOneShot(useClip);

        if (activationCountdown > 0)
        {
            StartActivationCountdown_Server();
        }
        else
        {
            ActivateItem();
        }
    }

    public virtual void StopUsing() { /* Override in subclasses */ }

    // activation countdown & UI
    // public virtual void StartActivationCountdown()
    // {
    //     // if (activationCoroutine != null) StopCoroutine(activationCoroutine);
    //     // activationCoroutine = StartCoroutine(ActivationCountdownRoutine());
    //     // Spawn the UI prefab if needed and initialize it with the starting seconds
    //     if (!countdownUIInstance && countdownUIPrefab)
    //     {
    //         // spawn at current target pos, not parented
    //         countdownUIInstance = Instantiate(countdownUIPrefab);
    //         countdownUIInstance.Init(this, Mathf.CeilToInt(activationCountdown));
    //     }
    //     else if (countdownUIInstance)
    //     {
    //         countdownUIInstance.Show();
    //         countdownUIInstance.SetCountdown(Mathf.CeilToInt(activationCountdown));
    //     }
    //
    //     isArmed = true;
    //
    //     if (activationCoroutine != null) StopCoroutine(activationCoroutine);
    //     activationCoroutine = StartCoroutine(ActivationCountdownRoutine());
    // }
    //
    // protected virtual IEnumerator ActivationCountdownRoutine()
    // {
    //     float t = activationCountdown;
    //     while (t > 0f)
    //     {
    //         if (countdownUIInstance)
    //             countdownUIInstance.SetCountdown(Mathf.CeilToInt(t));
    //
    //         if (audioSource && timerBeepClip) audioSource.PlayOneShot(timerBeepClip);
    //
    //         yield return new WaitForSeconds(1f);
    //         t -= 1f;
    //     }
    //
    //     if (countdownUIInstance) countdownUIInstance.SetCountdown(0);
    //
    //     if (audioSource && timerActivatedClip) audioSource.PlayOneShot(timerActivatedClip);
    //     if (countdownUIInstance) countdownUIInstance.Hide();
    //
    //     ActivateItem();
    // }
    
    public virtual void StartActivationCountdown_Server()
    {
        // Server-only timer logic; DO NOT spawn UI here
        if (activationCoroutine != null) StopCoroutine(activationCoroutine);
        activationCoroutine = StartCoroutine(ActivationCountdownRoutine_Server());
    }

    protected virtual IEnumerator ActivationCountdownRoutine_Server()
    {
        float t = activationCountdown;
        while (t > 0f)
        {
            // Optional: drive a NetworkVariable for time remaining if you want HUDs on all clients
            yield return new WaitForSeconds(1f);
            t -= 1f;
        }
        ActivateItem(); // This should be server-authoritative
    }

    public void StartActivationCountdown_LocalUI(int startSeconds)
    {
        // Owner-only local UI (non-networked)
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

        // Local UI countdown (purely cosmetic)
        if (activationUICoroutine != null) StopCoroutine(activationUICoroutine);
        activationUICoroutine = StartCoroutine(ActivationCountdownRoutine_LocalUI());
    }

    private Coroutine activationUICoroutine;

    private IEnumerator ActivationCountdownRoutine_LocalUI()
    {
        float t = activationCountdown;
        while (t > 0f)
        {
            countdownUIInstance?.SetCountdown(Mathf.CeilToInt(t));
            yield return new WaitForSeconds(1f);
            t -= 1f;
        }
        countdownUIInstance?.SetCountdown(0);
        countdownUIInstance?.Hide();
    }

    protected virtual void ActivateItem()
    {
        Debug.Log($"{gameObject.name} activated!");
    }

    public virtual void Disarm()
    {
        if (activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
            activationCoroutine = null;
        }
        DestroyCountdownUI();
        isArmed = false;
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
