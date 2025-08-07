using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

/// <summary>
/// Base for any inventory item you can use, arm (start a countdown), or launch (throw).
/// Inherit and override Use, OnArmed, Explode, etc for your specific item logic.
/// </summary>
public class UsableItem : MonoBehaviour, IPickup, IUsable
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
    [SerializeField] protected CountdownUI countdownUI; // drag the child UI here:D

    [Header("Launch Settings")]
    [SerializeField] protected float launchForce = 10f;
    [SerializeField] protected Vector3 launchDirection = Vector3.forward;
    protected Rigidbody rb;

    protected bool isArmed = false;
    protected Coroutine activationCoroutine;

    // track carrier/player
    public bool IsCarried { get; private set; }
    public Transform CurrentCarrier { get; private set; }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        // hide countdown by default
        if (countdownUI) countdownUI.Hide();

        // disable all colliders while held in inventory (start as pickupable)
        SetCollidersEnabled(true);
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
        CurrentCarrier = transform.parent;

        // Disable physics and colliders while in inventory
        rb.isKinematic = true;
        rb.useGravity = false;
        SetCollidersEnabled(false);

        if (countdownUI)
            countdownUI.Hide();
    }

    public virtual void Drop()
    {
        IsCarried = false;
        CurrentCarrier = null;

        // re-enable colliders and physics
        SetCollidersEnabled(true);
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (audioSource && dropClip) audioSource.PlayOneShot(dropClip);

        // uI will continue showing only if item is armed
    }

    // IUsable
    public virtual void Use()
    {
        if (audioSource && useClip) audioSource.PlayOneShot(useClip);

        if (activationCountdown > 0)
        {
            StartActivationCountdown();
        }
        else
        {
            ActivateItem();
        }
    }

    public virtual void StopUsing() { /* Override in subclasses */ }

    // activation countdown & UI
    public virtual void StartActivationCountdown()
    {
        if (activationCoroutine != null) StopCoroutine(activationCoroutine);
        activationCoroutine = StartCoroutine(ActivationCountdownRoutine());
    }

    protected virtual IEnumerator ActivationCountdownRoutine()
    {
        if (countdownUI)
            countdownUI.Show();

        float t = activationCountdown;
        while (t > 0)
        {
            if (countdownUI)
                countdownUI.SetCountdown(Mathf.CeilToInt(t));

            if (audioSource && timerBeepClip) audioSource.PlayOneShot(timerBeepClip);

            yield return new WaitForSeconds(1f);
            t -= 1f;
        }

        if (countdownUI)
            countdownUI.SetCountdown(0);

        if (audioSource && timerActivatedClip) audioSource.PlayOneShot(timerActivatedClip);

        if (countdownUI)
            countdownUI.Hide();

        ActivateItem();
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
        if (countdownUI)
            countdownUI.Hide();

        isArmed = false;
    }

    // Launch/drop (NOTE: don't call Drop() inside Drop(Vector3))
    public virtual void Launch(Vector3 direction, float force)
    {
        SetCollidersEnabled(true);
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(direction * force, ForceMode.VelocityChange);
    }

    public virtual void Drop(Vector3 dropPosition)
    {
        // re-enable physics and colliders
        SetCollidersEnabled(true);
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = dropPosition;

        if (audioSource && dropClip) audioSource.PlayOneShot(dropClip);

        IsCarried = false;
        CurrentCarrier = null;
        // Don't call Disarm(); in Drop(Vector3) because item may still be armed after throw/drop!
    }
}
