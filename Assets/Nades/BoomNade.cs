using Unity.VisualScripting;
using UnityEngine;


namespace Brad
{
    public class BoomNade : Nades
    {
      

        public override void Start()
        {
            base.Start();

            
            if (!hasLaunched) // Only launch if not already launched
            {
               
                Vector3 launchDirection = transform.forward; // Direction of launch 
                Launch(launchDirection); // Launch function from base class 
            }
        }

       // public override void Update()
       //  {
       //      countdown -= Time.deltaTime;
       //      if (countdown <= 0f)
       //      {
       //          Explode();
       //      }
       //  }
       //
       //
       //  void Explode()
       //  {
       //      Collider[] colliders = Physics.OverlapSphere(transform.position, grenadeData.explosionRadius);
       //      foreach (Collider collider in colliders)
       //      {
       //          Health health = collider.GetComponent<Health>();
       //          if (health != null)
       //          {
       //              health.TakeDamage(grenadeData.damage);
       //          }
       //      }
       //
       //      Destroy(gameObject);
       //  }


    }
}

