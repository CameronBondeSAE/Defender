using UnityEngine;

public class BasicGrenade : MonoBehaviour, IUsableItem
{
     [Header("Grenade Stats")]
    [SerializeField]
    private float explosionRadius = 5f;
    
    [SerializeField]
    private float explosionDelay = 3f;
    
    [SerializeField]
    private float launchSpeed = 10f;
    
    [SerializeField]
    private float damage = 50f;

    private float countdown;
    private Rigidbody rb;
    private bool isLaunched = false;

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
        if (isLaunched)
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0f) 
            {
                Explode();
            }
        }
    }

    /// <summary>
    /// Implementation of IUsableItem - called when player uses this item
    /// </summary>
    public void UseItem()
    {
        Launch();
    }

    private void Launch()
    {
        if (isLaunched) return;

        isLaunched = true;
        rb.isKinematic = false;
        rb.useGravity = true;
        var colliders = GetComponents<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = true;
        }
        transform.SetParent(null);
        Transform player = transform.root;
        Vector3 launchDirection = player.forward + Vector3.up * 0.5f;
        rb.AddForce(launchDirection * launchSpeed, ForceMode.VelocityChange);
        
        Debug.Log($"Grenade launched! Exploding in {countdown} seconds.");
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
                rb.AddExplosionForce(launchSpeed * 2f, transform.position, explosionRadius);
            }
        }

        // can add explosion effects here (particles, sound, etc.)
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw explosion radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}