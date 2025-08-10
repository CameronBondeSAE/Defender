using Defender;
using UnityEngine;

public class BasicGrenade : MonoBehaviour, IUsable, IPickup
{
   [Header("Grenade Stats")]
    private float explosionRadius = 5f;
    private float explosionDelay = 3f;
    private float damage = 50f;

    [Header("Explosion Force")]
    private float explosionForce = 10f;

    private float countdown;
    private Rigidbody rb;
    private bool isActivated = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        countdown = explosionDelay;
    }

    private void Update()
    {
        if (isActivated)
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0f) 
            {
                Explode();
            }
        }
    }

    /// <summary>
    /// UseItem from IUsableItem interface - called when player throws/uses this item
    /// This here just starts the countdown timer - throwing (rb.AddForce) is handled by PlayerCombat
    /// </summary>
    public void Use(CharacterBase characterTryingToUse)
    {
        ActivateGrenade();
    }

    public void StopUsing()
    {
	    
    }

    private void ActivateGrenade()
    {
        if (isActivated) return;
        
        isActivated = true;
        Debug.Log($"Grenade activated! Exploding in {countdown} seconds.");
    }

    private void Explode()
    {
        Debug.Log("Grenade exploded!");
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (var collider in colliders)
        {
            var health = collider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"Dealt {damage} damage to {collider.name}");
            }
            var rb = collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
        
        // Can add explosion effects here if you guys want (particles, sound, etc.)
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    public void Pickup()
    {
    }

    public void Drop()
    {
	    Use(GetComponent<CharacterBase>());
    }
}