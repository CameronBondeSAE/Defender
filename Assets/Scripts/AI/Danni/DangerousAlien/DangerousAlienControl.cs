using AIAnimation;
using Defender;
using UnityEngine;
using UnityEngine.AI;

public class DangerousAlienControl : CharacterBase
{
   public NavMeshAgent navAgent;
   public AIAnimationController animController;
   private AIAnimationController.AnimationState currentAnimState;
   public Transform playerTransform;
   
   [Header("Attack Settings")]
   public float damageMin = 0.5f;
   public float damageMax = 10f;
   public float attackRange = 2f;
   public float chaseRange = 15f;
   public float attackCooldownSeconds = 3f;
   public float attackDurationSeconds;
   public GameObject attackHitboxPrefab;
   public Transform attackHitboxRoot;
   [HideInInspector] GameObject attackHitboxInstance;
   public float nextAttackTime;

   [Header("Dodge Settings")] 
   public float explosiveSenseRadius = 10f;
   public float dodgeSampleRadius;
   public float dodgeSampleCount = 16f;
   public float dodgeMoveSpeed = 20f;
   public float normalMoveSpeed = 10f;
   public float dodgeArrivalTolerance = 0.5f;
   public LayerMask dodgeMask = ~0;

   [Header("Teamwork Settings")] 
   public float crateDefendRadius = 8f;
   public float allyAssistRadius;
   public float turretAssistRadius;

   [Header("Planner & Refs")] 
   public bool needsScan = true;
   public bool isMoving;
   public SmartAlienControl smartAlly;
   public NetworkedCrate allyCrateTarget;

   private void Awake()
   {
      navAgent = GetComponent<NavMeshAgent>();
      animController = GetComponent<AIAnimationController>();
      GameObject player = GameObject.FindGameObjectWithTag("Player");
      playerTransform = player.GetComponent<Transform>();
      smartAlly = FindObjectOfType<SmartAlienControl>();
      normalMoveSpeed = navAgent.speed;
   }

   private void SetupHitboxInstance()
   {
      if (attackHitboxInstance != null)
         return;
      if (attackHitboxPrefab != null && attackHitboxRoot != null)
      {
         attackHitboxInstance = Instantiate(
            attackHitboxPrefab, attackHitboxRoot.position,
            attackHitboxRoot.rotation, attackHitboxRoot
         );
         attackHitboxInstance.SetActive(false);
         // DangerousAlienHitBox hitbox = attackHitboxInstance.Getcomponent<DangerousAlienHitBox>;
         // if (hitBox != null) hitBox.owner = this;
      }
   }
   
   // attack helpers
   public bool CanAttackNow()
   {
      return Time.time >= nextAttackTime;
   }

   public void MarkAttackUsed()
   {
      nextAttackTime = Time.time + attackCooldownSeconds;
   }

   public float RollAttackDamage()
   {
      return Random.Range(damageMin, damageMax);
   }
   
   // ally helpers
   public bool TryFindSmartAlly()
   {
      if (smartAlly != null) return true;
      smartAlly = FindObjectOfType<SmartAlienControl>();
      return smartAlly != null;
   }

   public bool TryGetAllyCrateTarget(out NetworkedCrate crate)
   {
      crate = null;
      if(!TryFindSmartAlly()) return false;
      crate = smartAlly.currentCrateTarget;
      allyCrateTarget = crate;
      if (crate == null) return false;
      float distanceToCrate = Vector3.Distance(transform.position, crate.transform.position);
      return distanceToCrate <= crateDefendRadius;
   }

}
