using Unity.VisualScripting;
using UnityEngine;


namespace Brad
{
    public class BoomNade : Nades
    {
      

      public override void Start()
        {
            countdown = grenadeData.explosionDelay;
            launchSpeed = grenadeData.launchSpeed;
            //startPosition.position = transform.position;
            rb = GetComponent<Rigidbody>();
            Launch();
        }

       public override void Update()
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0f)
            {
                Explode();
            }
        }
       

        void Explode()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, grenadeData.explosionRadius);
            foreach (Collider collider in colliders)
            {
                Health health = collider.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(grenadeData.damage);
                }
            }

            Destroy(gameObject);
        }


    }
}