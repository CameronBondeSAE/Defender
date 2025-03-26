using UnityEngine;
using System;

public class TestHealth : MonoBehaviour
{
    // events other classes can subscribe to; trigger when health is modified/reaches 0/restored
    public event Action<float> OnHealthChanged;

    public float maxHealth;
    public float currentHealth;
    public bool isDead = false;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (isDead) return;
        OnHealthChanged?.Invoke(currentHealth);
        if (currentHealth <= 0) Destroy(gameObject);
    }

    public void TakeSomeDamage()
    {
        TakeDamage(5);
    }
    
    public void TakeBigDamage()
    {
        TakeDamage(50);
    }
}
