using Unity.VisualScripting;
using UnityEngine;

public class Grenade : MonoBehaviour
{
   public GrenadeData grenadeData;
   private float countdown;
   private float launchSpeed;
   private Rigidbody rb;
   private Transform startPosition;

   void Start()
   {
      countdown = grenadeData.explosionDelay;
      launchSpeed = grenadeData.launchSpeed;
      startPosition.position = transform.position;
      rb = GetComponent<Rigidbody>();
      Launch();
   }
   
   void Update()
   {
      countdown -= Time.deltaTime;
      if (countdown <= 0f)
      {
         Explode();
      }
   }

   public void Launch()
   {
      Vector3 launchDirection = startPosition.forward +startPosition.up * 1f;
      rb.AddForce(launchDirection * launchSpeed, ForceMode.VelocityChange);
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
