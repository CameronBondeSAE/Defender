using Unity.Netcode;
using UnityEngine;

public class SnackObject : MonoBehaviour
{
   public float biteDamage = 10f;
   public float maxLifetime = 0f;

   public SnackHealth snackHealth;

   private float lifetimeTimer = 0f;
   private UsableItem_Base parentItem;

   private void Awake()
   {
      snackHealth = GetComponent<SnackHealth>();
      parentItem  = GetComponentInParent<UsableItem_Base>();
   }

   private void Update()
   {
      if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer)
         return;

      if (snackHealth == null)
         return;
      if (maxLifetime > 0f && !snackHealth.isDead)
      {
         lifetimeTimer += Time.deltaTime;
         if (lifetimeTimer >= maxLifetime)
         {
            snackHealth.TakeDamage(snackHealth.currentHealth.Value); 
         }
      }
      if (snackHealth.isDead && parentItem != null)
      {
         parentItem.DestroyItem(); 
      }
   }
   public bool TakeBite()
   {
      if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsServer) return false;
      if (snackHealth == null || snackHealth.isDead) return false;

      snackHealth.TakeDamage(biteDamage);
      return !snackHealth.isDead;
   }
}
