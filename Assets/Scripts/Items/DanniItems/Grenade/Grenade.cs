using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.WSA;

public class Grenade : UsableItem
{
    [Header("Grenade Stats")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionDelay = 3f;
    [SerializeField] private float damage = 50f;
    //[SerializeField] private float throwForce = 8f;
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
    public override void Use()
    {
        Debug.Log("Grenade Activated");
        Arm(explosionDelay);
        Launch(transform.forward, launchForce);
    }

    protected override void OnArmed()
    {
        base.OnArmed();
        countdown = explosionDelay;
        isActivated = true;
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var col in colliders)
        {
            Debug.Log("Grenade exploded");
            var health = col.GetComponent<Health>();
            if(health != null) health.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
