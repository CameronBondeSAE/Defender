using UnityEngine;
using AIAnimation;
using System.Collections;

public class AttackState : MonoBehaviour, IAIState
{
    private AIBase ai;
    private AIAnimationController animController;

    [Header("Attack Settings")]
    public float attackDistance = 2f;
    public float alertDistance = 10f;
    public float attackCooldown = 5f;
    public float attackDuration = 1f;

    private GameObject attackHitbox;
    private Transform player;
    private bool isAttacking;
    private float nextAttackTime;

    public AttackState(AgroAlienAI ai) => this.ai = ai;

    public void Enter()
    {
        if (animController == null)
            animController = ai.GetComponentInChildren<AIAnimationController>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (attackHitbox == null)
        {
            Transform hitboxTransform = ai.transform.Find("HitBox");
            if (hitboxTransform != null)
                attackHitbox = hitboxTransform.gameObject;
            else
                Debug.LogWarning("HitBox object not found under AI.");
        }
    }

    public void Stay()
    {
        if (player == null || attackHitbox == null)
            return;

        float distanceToPlayer = Vector3.Distance(ai.transform.position, player.position);

        if (distanceToPlayer > alertDistance)
        {
            ai.ChangeState(new PatrolState(ai));
            return;
        }

        if (!isAttacking && Time.time >= nextAttackTime && distanceToPlayer <= attackDistance)
        {
            ai.StopMoving();
            ai.StartCoroutine(PerformAttack());
        }
        else if (!isAttacking)
        {
            ai.MoveTo(player.position);
        }
    }

    public void Exit()
    {
        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        isAttacking = false;
        ai.StopAllCoroutines();
    }

    // consecutive attack coroutine
    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        animController.SetAnimation(AIAnimationController.AnimationState.Attack);

        if (attackHitbox != null)
            attackHitbox.SetActive(true);

        yield return new WaitForSeconds(attackDuration);

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        yield return new WaitForSeconds(attackCooldown - attackDuration);
        isAttacking = false;
        nextAttackTime = Time.time + attackCooldown;
    }
}

