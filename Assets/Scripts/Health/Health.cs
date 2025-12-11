using UnityEngine;
using System;
using Unity.Netcode;

public class Health : NetworkBehaviour
{
   // events other classes can subscribe to; trigger when health is modified/reaches 0/restored
   public event Action OnDeath;
   public event Action<float> OnHealthChanged;
   public event Action OnRevive;
   
   public float maxHealth;
   public float revivedHealth;
   public NetworkVariable<float> currentHealth = new NetworkVariable<float>();
   public bool isDead = false;

   [Header("Health related animation Params")] 
   public float deathAnimDuration;
   //public float hitAnimDuration;
   
   protected virtual void Awake()
   {
      currentHealth.Value = maxHealth;
   }

   public virtual void TakeDamage(float amount)
   {
      currentHealth.Value -= amount;
      if (isDead) return;
      OnHealthChanged?.Invoke(amount);
      if (currentHealth.Value <= 0) Die();
   }

   public virtual void Heal(float amount)
   {
      if (isDead) return;
      currentHealth.Value = Mathf.Min(currentHealth.Value + amount, maxHealth); // does not heal over maxHealth
      OnHealthChanged?.Invoke(currentHealth.Value);
   }

   // for 2-player mode?
   public virtual void Revive()
   {
      if (!isDead) return;
      isDead = false;
      currentHealth.Value = revivedHealth;
      OnRevive?.Invoke();
      OnHealthChanged?.Invoke(currentHealth.Value);
   }

   protected virtual void Die()
   {
      if(isDead) return;
      isDead = true;
      OnDeath?.Invoke();
   }
   
   
   // WILAYAT: need to add this method so that clients can request server to apply damage to anything that has health component
   [Rpc(SendTo.Server, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
   public void TakeDamage_ServerRpc(float amount)
   {
      TakeDamage(amount);
   }
}
