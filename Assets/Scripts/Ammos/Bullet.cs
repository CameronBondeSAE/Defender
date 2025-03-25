using UnityEngine;

public class Bullet : MonoBehaviour
{
    public BulletData bulletData;
    public Transform spawnPosition;
    void Start()
    {
        GetComponent<Rigidbody>().linearVelocity = transform.forward * bulletData.speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(spawnPosition.position, transform.position) > bulletData.range)
        {
            Destroy(gameObject);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Health health = collision.gameObject.GetComponent<Health>();
        // if (health != null)
        // {
        //     health.TakeDamage(bulletData.damage); 
        // }

        Destroy(gameObject); 
    }
}
