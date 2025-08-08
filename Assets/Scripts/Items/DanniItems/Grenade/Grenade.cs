using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.WSA;

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
    }
    
    // In Grenade
    public override void Pickup()
    {
        base.Pickup(); // plays audio, sets IsCarried, disables physics
        // detect and store the carrier
        var player = FindObjectOfType<PlayerHealth>();
        if (player != null)
        {
            SetCarrier(player.transform);
        }
        Debug.Log("Grenade picked up and carrier set.");
    }


    public override void Use()
    {
        Debug.Log("Grenade thrown!");
        // Launch itself forward (when used)
        Launch(CurrentCarrier.forward, launchForce);
        base.Use(); // starts the activation countdown
    }

    protected override void ActivateItem()
    {
        Explode();
    }

    private void Explode()
    {
        if (explosionEffect)
        {
            var vfx = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(vfx.gameObject, vfx.main.duration);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var col in colliders)
        {
            var health = col.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(damage);
        }
        Debug.Log("grenade exploded");
        Destroy(gameObject);
    }
}
