using UnityEngine;

public class SnackObject : MonoBehaviour
{
   public float biteDamage = 10f;
   public float maxLifetime = 0f;
   public SnackHealth snackHealth;
   private float lifetimer = 0f;

   private void Awake()
   {
      snackHealth = GetComponent<SnackHealth>();
   }

   private void Update()
   {
      if (maxLifetime > 0f && !snackHealth.isDead)
      {
         lifetimer += Time.deltaTime;
         if(lifetimer >= maxLifetime)
            snackHealth.TakeDamage(snackHealth.currentHealth.Value);
      }
   }

   public bool TakeBite()
   {
      if (snackHealth == null || snackHealth.isDead) return false;
      snackHealth.TakeDamage(biteDamage);
      return !snackHealth.isDead;
   }
}
