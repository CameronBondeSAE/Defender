using Defender;
using Unity.Netcode;
using UnityEngine;

public class Grenade : UsableItem_Base
{
    [Header("Grenade Stats")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float damage = 50f;
    [SerializeField] private float explosionForce = 10f;
    [SerializeField] private float grenadeCountdown = 3f;
    
    [Header("Explosion effect")]
    public ParticleSystem explosionEffect;

    protected override void Awake()
    {
        base.Awake();
        activationCountdown = grenadeCountdown;
        expiryDuration = 0f;
    }
    
    // In Grenade
    public override void Pickup(CharacterBase whoIsPickupMeUp)
    {
        base.Pickup(whoIsPickupMeUp); // plays audio, sets IsCarried, disables physics
        // detect and store the carrier
        Debug.Log("Grenade picked up");
        // if (IsServer)
        // {
        //     GrenadePickupClientRpc();
        // }
    }
    // [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    // private void GrenadePickupClientRpc()
    // {
    //     
    // }

    public override void Use(CharacterBase characterTryingToUse)
    {
        Debug.Log($"[Grenade.Use] Grenade used by {characterTryingToUse?.name}");
        
        // server launch and inventory clearing
        if (IsServer && characterTryingToUse != null)
        {
            Vector3 launchDirection = characterTryingToUse.transform.forward;
            
            // clear from inventory BEFORE launching
            PlayerInventory inventory = characterTryingToUse.GetComponent<PlayerInventory>();
            if (inventory != null && inventory.CurrentItemInstance == this.gameObject)
            {
                Debug.Log("[Grenade.Use] Clearing grenade from inventory before launch");
                inventory.ClearCurrentItemWithoutDestroy();
            }
            LaunchGrenade(launchDirection, launchForce);
        }
        base.Use(characterTryingToUse);
    }

    private void LaunchGrenade(Vector3 direction, float force)
    {
        Launch(direction, force);
        // sync to clients
        LaunchGrenadeClientRpc(direction, force);
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    private void LaunchGrenadeClientRpc(Vector3 direction, float force)
    {
        Debug.Log($"[Grenade] Syncing grenade launch to client");
        if (!IsServer)
        {
            Launch(direction, force);
        }
    }
    protected override void ActivateItem()
    {
        base.ActivateItem();
        if (IsServer)
        {
            ExplodeServer();
        }
    }

    private void ExplodeServer()
    {
        Debug.Log("[ExplodeServer] Grenade exploding on server!");
        // do damage calculation on server FIRST
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var col in colliders)
        {
            var health = col.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"[ExplodeServer] Dealt {damage} damage to {col.name}");
            }
        }
        // sync explosion effects to all clients
        ExplodeClientRpc(transform.position);
        
        // networked destruction instead of Destroy()
        DestroyItem_Server();
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    private void ExplodeClientRpc(Vector3 explosionPosition)
    {
        Debug.Log($"[ExplodeClientRpc] Grenade explosion effect at {explosionPosition} on {(IsServer ? "Server" : "Client")}");
        if (explosionEffect)
        {
            var explosionParticles = Instantiate(explosionEffect, explosionPosition, Quaternion.identity);
            
            float duration = explosionEffect.main.duration;
            Destroy(explosionParticles.gameObject, duration);
        }
        else
        {
        }
    }
}
