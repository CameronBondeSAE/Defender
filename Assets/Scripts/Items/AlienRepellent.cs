using UnityEngine;
using Defender;
using System.Collections;


public class AlienRepellent : UsableItem_Base
{
   [Header("Repellent Settings")]
   [SerializeField] private float repellentRadius = 15f;
   [SerializeField] private float activeDuration = 10f;
   [SerializeField] private float rechargeDuration = 15f;

   [Header("Audio")]
   [SerializeField] private AudioClip deterrentSound;
   [SerializeField] private AudioClip placementSound;
   
   private AudioSource audioSource;
   private bool isActive = true;
   private bool isPlaced = false;
   private float pulseTimer = 0;
   private float pulseInterval = 1f;

   protected override void Awake()
   {
      base.Awake();
      audioSource = GetComponent<AudioSource>();
      if (audioSource == null)
      {
         audioSource = gameObject.AddComponent<AudioSource>();
      }
   }
   
   public override void Pickup(CharacterBase whoIsPickupMeUp)
   {
      base.Pickup(whoIsPickupMeUp);
      Debug.Log("AlienRepellent picked up");
   }

   public override void Use(CharacterBase characterTryingToUse)
   {
      if(!isActive || isPlaced) return;
      
      base.Use(characterTryingToUse);
      
      isPlaced = true;
      transform.parent = null;

      if (placementSound != null)
      {
         audioSource.PlayOneShot(placementSound);
      }

      StartCoroutine(RepellentRoutine());
   }
   
   private IEnumerator RepellentRoutine()
   {
      float activeTime = 0f;
      if (deterrentSound != null)
      {
         audioSource.clip = deterrentSound;
         audioSource.loop = true;
         audioSource.Play();
      }

      while (activeTime < activeDuration)
      {
         PulseEffect();
         activeTime += pulseInterval;
         yield return new WaitForSeconds(pulseInterval);
      }
      
      isActive = false;
      audioSource.Stop();

      GetComponent<Collider>().enabled = true;
      GetComponent<MeshRenderer>().enabled = true;
   }
   
   private void PulseEffect()
   {
      AlertNearbyAliens();
   }
   
   private void AlertNearbyAliens()
   {
      Collider[] hitColliders = Physics.OverlapSphere(transform.position, repellentRadius);
      foreach (Collider hitCollider in hitColliders)
      {
         AlienAI alien = hitCollider.GetComponent<AlienAI>();
         if (alien != null)
         {
            alien.FleeFromPosition(transform.position, repellentRadius, activeDuration);
         }
      }
   }

   void OnDrawGizmosSelected()
   {
      Gizmos.color = Color.red;
      Gizmos.DrawWireSphere(transform.position, repellentRadius);
   }
}
