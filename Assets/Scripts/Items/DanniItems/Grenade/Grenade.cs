using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.WSA;

public class Grenade : UsableItem
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

    public override void Use()
    {
        Debug.Log("Grenade thrown!");
        // Launch itself forward (when used)
        Launch(transform.forward, launchForce);
        base.Use(); // starts the activation countdown
    }

    public override void Launch(Vector3 direction, float force)
    {
        SetCollidersEnabled(true);
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(direction * force, ForceMode.VelocityChange);
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
