using UnityEngine;

/// <summary>
/// modular hitbox/DoDamage script
/// </summary>
public class DangerousAlienHitBox : MonoBehaviour
{
    public DangerousAlienControl owner;

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider other)
    {
        if (owner == null)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            float damage = owner.RollAttackDamage(); // keep damage logic on the owner for reusability
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}