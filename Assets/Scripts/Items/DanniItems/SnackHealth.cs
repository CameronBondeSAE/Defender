using UnityEngine;

public class SnackHealth : Health
{
   public float destroyDelay = 0.3f;
   public GameObject destroyEffect;
   protected override void Die()
   {
      base.Die();
      if(destroyEffect != null)
         Instantiate(destroyEffect, transform.position, Quaternion.identity);
      // hide mesh & collider so civs don't bump into them & get stuck
      Collider[] colliders = GetComponentsInChildren<Collider>(true);
      for (int i = 0; i < colliders.Length; i++)
         colliders[i].enabled = false;
      MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);
      for (int i = 0; i < renderers.Length; i++)
         renderers[i].enabled = false;
      
      Destroy(gameObject, destroyDelay);
   }
}
