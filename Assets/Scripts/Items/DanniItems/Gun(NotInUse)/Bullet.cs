using UnityEngine;

public partial class Bullet : MonoBehaviour
{
    public BulletData bulletData;
    private Rigidbody rb;
    private Vector3 startPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        rb.linearVelocity = transform.forward * bulletData.speed;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Vector3.Distance(startPosition, transform.position) > bulletData.range) Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var health = collision.gameObject.GetComponent<Health>();
        if (health != null)
        {
            Debug.Log(collision.gameObject.name);
            health.TakeDamage(bulletData.damage);
        }

        Destroy(gameObject);
    }
}