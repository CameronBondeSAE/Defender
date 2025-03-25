using UnityEngine;

public class Bullet : MonoBehaviour
{
    public BulletData bulletData;
    private Vector3 startPosition;
    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position; 
        rb.linearVelocity = transform.forward * bulletData.speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(startPosition, transform.position) > bulletData.range)
        {
            Destroy(gameObject);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        Health health = collision.gameObject.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(bulletData.damage); 
        }

        Destroy(gameObject); 
    }
}
