using UnityEngine;

namespace Jasper_AI
{
    public class DamageShot : Projectile
    {
        private void OnTriggerEnter(Collider other)
        {
            //if hits something with a health component, heal and destroy self 
            if (other.TryGetComponent(out Health health))
            {
                health.TakeDamage(strength);
                Destroy(gameObject);
            }
        }
    }
}
