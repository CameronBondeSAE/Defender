using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Base for any inventory item you can use, arm (start a countdown), or launch (throw).
/// Inherit and override Use, OnArmed, Explode, etc for your specific item logic.
/// </summary>
public class UsableItem : MonoBehaviour, IUsable
{
    [Header("Item Settings")]
    [SerializeField] private bool isConsumable = true;
    public virtual bool IsConsumable => isConsumable;

    [Header("Arming Settings")]
    [SerializeField] protected float armDuration = 2f; 
    protected bool isArmed = false;
    protected Coroutine armingCoroutine;

    [Header("Launch Settings")]
    [SerializeField] public float launchForce = 10f;
    [SerializeField] protected Vector3 launchDirection = Vector3.forward; 
    protected Rigidbody rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true; // non-kinematic when launched
        rb.useGravity = false;
    }

    public virtual void Use()
    {
        // your own item logic. you can call Arm() here, or Launch() etc based on what your item is
        // or if consumable, directly manipulate player's health etc here
    }
    public virtual void StopUsing()
    {
        Arm(armDuration);
    }

    public virtual void Arm(float duration)
    {
        if(isArmed) return;
        if (duration > 0f) armDuration = duration;
        if(armingCoroutine != null) StopCoroutine(armingCoroutine);
        armingCoroutine = StartCoroutine(ArmRoutine());
    }

    protected virtual IEnumerator ArmRoutine()
    {
        isArmed = true;
        yield return new WaitForSeconds(armDuration);
        OnArmed();
    }

    public virtual void Disarm()
    {
        isArmed = false;
        if (armingCoroutine != null)
        {
            StopCoroutine(armingCoroutine);
            armingCoroutine = null;
        }
    }

    protected virtual void OnArmed()
    {
        // custom logic
    }

    public virtual void Launch(Vector3 direction, float force)
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.AddForce(direction * force);
    }

    public virtual void Drop(Vector3 dropPosition)
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        transform.position = dropPosition;
    }
    
}
