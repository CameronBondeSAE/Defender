using Defender;
using Unity.Netcode;
using UnityEngine;

public class PushGrenade : UsableItem_Base
{
    [Header("Grenade Stats")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 10f;
    [SerializeField] private float grenadeCountdown = 3f;
    

    protected override void Awake()
    {
        base.Awake();
        activationCountdown = grenadeCountdown;
        expiryDuration = 0f;
    }

    public override void Pickup(CharacterBase whoIsPickupMeUp)
    {
        base.Pickup(whoIsPickupMeUp);
        Debug.Log("Grenade picked up");
    }

    public override void Use(CharacterBase characterTryingToUse)
    {
        Debug.Log($"[GrenadePushAll.Use] Used by {characterTryingToUse?.name}");

        if (IsServer && characterTryingToUse != null)
        {
            Vector3 launchDirection = characterTryingToUse.transform.forward;

            PlayerInventory inventory = characterTryingToUse.GetComponent<PlayerInventory>();
            if (inventory != null && inventory.CurrentItemInstance == this.gameObject)
            {
                inventory.ClearCurrentItemWithoutDestroy();
            }

            LaunchGrenade(launchDirection, launchForce);
        }

        base.Use(characterTryingToUse);
    }

    private void LaunchGrenade(Vector3 direction, float force)
    {
        Launch(direction, force);
        LaunchGrenadeClientRpc(direction, force);
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable)]
    private void LaunchGrenadeClientRpc(Vector3 direction, float force)
    {
        if (!IsServer)
            Launch(direction, force);
    }

    protected override void ActivateItem()
    {
        base.ActivateItem();
        if (IsServer)
            ExplodeServer();
    }

    private void ExplodeServer()
    {
        Debug.Log("[ExplodeServer] Grenade exploding on server!");

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var col in colliders)
        {
            Vector3 direction = (col.transform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, col.transform.position);
            float forceMultiplier = 1 - (distance / explosionRadius); // falloff

          
            // Rigidbody push
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce * forceMultiplier, transform.position, explosionRadius, 1f, ForceMode.Impulse);
            }
            else
            {
                // CharacterController push
                var cc = col.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.Move(direction * explosionForce * forceMultiplier * Time.fixedDeltaTime);
                }
            }
        }
        
        DestroyItem_Server();
    }
}


