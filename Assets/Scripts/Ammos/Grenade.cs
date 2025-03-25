using Unity.VisualScripting;
using UnityEngine;

public class Grenade : MonoBehaviour
{
   public GrenadeData grenadeData;
   private float countdown;
   public float launchSpeed;

   void Start()
   {
      countdown = grenadeData.explosionDelay;
      launchSpeed = grenadeData.speed;
   }
   
   void Update()
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
