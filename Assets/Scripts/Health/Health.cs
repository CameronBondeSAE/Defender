using UnityEngine;
using System;

public class Health : MonoBehaviour
{
   // events other classes can subscribe to; trigger when health is modified/reaches 0/restored
   public event Action OnDeath;
   public event Action<float> OnHealthChanged;
   public event Action OnRevive;
   
   public float maxHealth;
   public float revivedHealth;
   public float currentHealth;
   public bool isDead = false;

   [Header("Health related animation Params")] 
   public float deathAnimDuration;
   //public float hitAnimDuration;
   
   protected virtual void Awake()
   {
      currentHealth = maxHealth;
   }

   public virtual void TakeDamage(float amount)
   {
      currentHealth -= amount;
      if (isDead) return;
      OnHealthChanged?.Invoke(amount);
      if (currentHealth <= 0) Die();
   }

   public virtual void Heal(float amount)
   {
      if (isDead) return;
      currentHealth = Mathf.Min(currentHealth + amount, maxHealth); // does not heal over maxHealth
      OnHealthChanged?.Invoke(currentHealth);
   }

   // for 2-player mode?
   public virtual void Revive()
   {
      if (!isDead) return;
      isDead = false;
      currentHealth = revivedHealth;
      OnRevive?.Invoke();
      OnHealthChanged?.Invoke(currentHealth);
   }

   protected virtual void Die()
   {
      isDead = true;
      if(isDead) return;
      OnDeath?.Invoke();
   }
}
