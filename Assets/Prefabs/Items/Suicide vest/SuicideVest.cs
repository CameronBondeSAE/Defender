using Defender;
using System.Buffers;
using System.Collections;
using TreeEditor;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// on pickup, the vest is not activated.
/// oncollision enter, attach to the other character (not the owner)
/// </summary>
public class SuicideVest : UsableItem_Base
{
    [Space]
    [Header("Vest Settings")]
    [SerializeField] private float damageAmount;
    [SerializeField] private float explosionRadius;
    [SerializeField] private Transform vestTransform;
    [SerializeField] private CharacterBase owner;
    [SerializeField] private DrawSphereOfInfluence explosionRadiusVisual;
    private enum VestState { disabled, inHand, isAttached };
    [SerializeField] private VestState state;

    [SerializeField] private GameObject sparkParticles;

    public CharacterBase entityAttachedTo;
    
    public Collider vestTrigger;
    
    // your manual countdown length (don’t rely on base.activationCountdown here!)
    [SerializeField] private float attachActivationDelay = 5f;

    // fix: follow the attached target server-side only? let NetworkTransform replicate
    private void LateUpdate()
    {
        if (!IsServer) return;

        if (state == VestState.isAttached && entityAttachedTo != null)
        {
            // same offset math you had
            transform.rotation = entityAttachedTo.transform.rotation;
            transform.position = entityAttachedTo.transform.position
                                 + transform.forward * -0.8f
                                 + transform.up * 1.5f;
        }
    }

    public override void StopUsing()
    {
        base.StopUsing();

        state = VestState.disabled;

        Debug.Log("Vest disabled");
    }

    public override void Pickup(CharacterBase whoIsPickupMeUp)
    {
        base.Pickup(whoIsPickupMeUp);

        state = VestState.inHand;
        
        owner = whoIsPickupMeUp;
        vestTrigger.enabled = true;
        
        //Use(whoIsPickupMeUp);
    }

    public override void Use(CharacterBase characterTryingToUse)
    {
        //---------------------------Old Code----------------------------------
        // base.Use(characterTryingToUse);

        // Launch(characterTryingToUse.transform.forward, launchForce); 

        // on pickup, the vest is not activated. instead it is assigned an owner
        // on collision enter, attach the vest to the other character (whether that be a civ, alien, or player)
        //---------------------------Old Code----------------------------------
        
        // fix: do NOT call base.Use() so OnUse mode in the base can’t start activation here.
        // play use sfx manually (this preserves your audio behavior without triggering activation)
        if (audioSource && useClip) audioSource.PlayOneShot(useClip);

        // on pickup, the vest is not activated. instead it is assigned an owner
        // on collision enter, attach the vest to the other character (whether that be a civ, alien, or player)
    }

    // fix: handle attach SERVER-SIDE ONLY so activation is authoritative
    private void OnTriggerEnter(Collider collision)
    {
        if (!IsServer) return;
        if (state != VestState.inHand) return;
        if (!collision) return;

        var targetChar = collision.GetComponent<CharacterBase>();
        if (!targetChar) return;

        // don’t attach to the owner
        if (owner && targetChar.gameObject == owner.gameObject) return;

        // tell owner’s inventory to drop it (your/cam's existing HACK preserved with guards)...?
        var ownerInv = owner ? owner.GetComponent<PlayerInventory>() : null;
        if (ownerInv) ownerInv.DropHeldItem();

        entityAttachedTo = targetChar;
        state = VestState.isAttached;

        // visuals on all clients
        SetAttachedVisualsClientRpc(true);

        // disable further trigger spam + physical interactions after attach
        if (vestTrigger) vestTrigger.enabled = false;
        SetCollidersEnabled(false);

        // fix: start the manual activation countdown now that it's attached
        // server path uses this overload, clients would call TryStartActivation(attachActivationDelay)
        StartActivationCountdown_Server(attachActivationDelay);
    }
    
     // fix: explosion happens WHEN the countdown completes - i.e., when base calls ActivateItem()
    protected override void ActivateItem()
    {
        // let the base mark activated & clear activation vars; no expiry is needed for a vest (so keep expiryDuration = 0)
        base.ActivateItem();
        if (!IsServer) return;

        // small guard: only explode if actually attached
        if (state != VestState.isAttached || entityAttachedTo == null)
        {
            // if somehow activated without attach, just self-destroy to be safe
            DestroyItem();
            return;
        }

        // play client-side effects
        PlayExplosionEffectsClientRpc();

        // damage everything in range (including the attached target)
        const int maxHits = 32;
        Collider[] hits = new Collider[maxHits];
        int count = Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, hits);

        for (int i = 0; i < count; i++)
        {
            var col = hits[i];
            if (!col) continue;

            var cb = col.GetComponent<CharacterBase>();
            if (!cb) continue;

            var hp = cb.GetComponent<Health>();
            if (hp) hp.TakeDamage(damageAmount);
        }

        Debug.Log("Exploded");

        // cleanly despawn via base (clears netvars and despawns NetworkObject)
        DestroyItem();
    }

    // fix: toggle “armed/attached” visuals on all clients
    [Rpc(SendTo.Everyone, Delivery = RpcDelivery.Reliable)]
    private void SetAttachedVisualsClientRpc(bool on)
    {
        if (sparkParticles) sparkParticles.SetActive(on);
        if (explosionRadiusVisual) explosionRadiusVisual.enabled = on;
    }

    // play one-shot explosion feedback on clients
    [Rpc(SendTo.Everyone, Delivery = RpcDelivery.Reliable)]
    private void PlayExplosionEffectsClientRpc()
    {
        // turn off the radius ring just before/at explosion
        if (explosionRadiusVisual) explosionRadiusVisual.enabled = false;
        if (sparkParticles) sparkParticles.SetActive(false);

        // you can spawn particles/sound here if you wantt (CLIENT-ONLY visual/audio)
    }

    // private IEnumerator Explode()
    // {
    //     // once attached, explode after a set timer
    //
    //     if (state == VestState.isAttached)
    //     {
    //         sparkParticles.SetActive(true);
    //         explosionRadiusVisual.enabled = true;
    //
    //         activationCountdown = 5f;
    //         StartActivationCountdown_Server();
    //
    //         SetCollidersEnabled(false);
    //         
    //         yield return new WaitForSeconds(activationCountdown);
    //
    //         Collider[] collidersInRange = new Collider[10];
    //         Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, collidersInRange);
    //
    //         foreach (Collider collider in collidersInRange)
    //         {
    //             if (collider != null)
    //             {
    //                 if (collider.GetComponent<CharacterBase>())
    //                 {
    //                     collider.gameObject.GetComponent<Health>().TakeDamage(damageAmount);
    //                 }
    //             }
    //             else
    //             {
    //                 continue;
    //             }
    //         }
    //
    //         Debug.Log("Exploded");
    //
    //         gameObject.GetComponent<NetworkObject>().Despawn();
    //     }
    // }
}
