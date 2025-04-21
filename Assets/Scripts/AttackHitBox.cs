using UnityEngine;

public class AttackHitBox : MonoBehaviour
{
    private AgroAlienAI ai;

    private void Start()
    {
        ai = GetComponentInParent<AgroAlienAI>();
        if (ai == null)
        {
            Debug.LogError("Doesn't have AgroAlienAI on parent!");
        }
    }

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
        if (other.CompareTag("Player"))
        {
            float damage = Random.Range(ai.damageMin, ai.damageMax);
            /*PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }*/
        }
    }
}
