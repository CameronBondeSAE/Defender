using UnityEngine;

public class ExplodingBarrel : MonoBehaviour
{
    public float explosionRadius = 5f;
    public float explodeDamage = 700f;
    public float damage = 100f;
}

    private Health health;

    void Awake();
    {
        health = GetComponent<Health>();
        health.OnDeath += Explode;
    }

    void onDestroy();
    {
        health.OnDeath -= Explode;
    }

    void Explode()
    {
        
    }