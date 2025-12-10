using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    /// <summary>
    /// Old monobehaviour bullet code
    /// </summary>
    // public BulletData bulletData;
    // private Rigidbody rb;
    // private Vector3 startPosition;
    //
    // private void Start()
    // {
    //     rb = GetComponent<Rigidbody>();
    //     startPosition = transform.position;
    //     rb.linearVelocity = transform.forward * bulletData.speed;
    // }
    // private void Update()
    // {
    //     if (Vector3.Distance(startPosition, transform.position) > bulletData.range) Destroy(gameObject);
    // }
    //
    // private void OnCollisionEnter(Collision collision)
    // {
    //     
    //     var health = collision.gameObject.GetComponent<Health>();
    //     if (health != null)
    //     {
    //         Debug.Log(collision.gameObject.name);
    //         health.TakeDamage(bulletData.damage);
    //     }
    //     Destroy(gameObject);
    // }
    
    public BulletData bulletData;

    private Rigidbody rb;
    private Vector3 startPosition;
    private bool initialized;

    public override void OnNetworkSpawn()
    {
        if (IsServer) InitializeServer();
    }

    private void InitializeServer()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.None;
        rb.isKinematic = false;
        rb.linearVelocity = transform.forward * bulletData.speed;
        startPosition = transform.position;
        initialized = true;
    }

    private void FixedUpdate()
    {
        if (!IsServer || !initialized) return;
        rb.linearVelocity = transform.forward * bulletData.speed;
        if (Vector3.Distance(startPosition, transform.position) > bulletData.range)
            Despawn();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        var other = collision?.gameObject;
        if (other != null)
        {
            var health = other.GetComponent<Health>();
            if (health != null) health.TakeDamage(bulletData.damage);
        }

        Despawn();
    }

    private void Despawn()
    {
        var no = GetComponent<NetworkObject>();
        if (no != null && no.IsSpawned) no.Despawn(true);
        else Destroy(gameObject);
    }
}