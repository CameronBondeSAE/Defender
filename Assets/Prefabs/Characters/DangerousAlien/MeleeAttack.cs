using UnityEngine;
using Anthill.AI;
using AIAnimation;
using UnityEngine.AI;
using System.Collections;
using Defender;
/// <summary>
/// In this state he will stop moving, play the attack animation, enable the hitbox for the swing window, then disable it again and wait out the cooldown.
/// </summary>
public class MeleeAttack : AntAIState
{
    private DangerousAlienControl control;
    private NavMeshAgent navMeshAgent;
    private AIAnimationController animationController;

    private bool attackStarted;
    private Coroutine attackCoroutine;

    public override void Create(GameObject aGameObject)
    {
        control            = aGameObject.GetComponent<DangerousAlienControl>();
        navMeshAgent       = aGameObject.GetComponent<NavMeshAgent>();
        animationController = aGameObject.GetComponentInChildren<AIAnimationController>();
    }

    public override void Enter()
    {
        if (control == null || navMeshAgent == null || animationController == null)
        {
            Finish();
            return;
        }

        if (!control.CanAttackNow())
        {
            Finish();
            return;
        }

        if (control.playerTransform == null)
        {
            Finish();
            return;
        }

        navMeshAgent.isStopped = true;
        attackStarted = true;

       CharacterBase host = control; 
        attackCoroutine = host.StartCoroutine(AttackRoutine());
    }

    // I'm starting the coroutine through control because AntAIState itself isn’t a MonoBehaviour
    private IEnumerator AttackRoutine()
    {
        animationController.SetAnimation(AIAnimationController.AnimationState.Attack);

        if (control.attackHitboxInstance != null)
        {
            control.attackHitboxInstance.SetActive(true);
        }

        yield return new WaitForSeconds(control.attackDurationSeconds);

        if (control.attackHitboxInstance != null)
        {
            control.attackHitboxInstance.SetActive(false);
        }

        control.MarkAttackUsed();

        yield return new WaitForSeconds(
            Mathf.Max(0f,
            control.attackCooldownSeconds - control.attackDurationSeconds));

        attackStarted = false;
        Finish();
    }

    public override void Execute(float aDeltaTime, float aTimeScale)
    {
        if (!attackStarted)
        {
            Finish();
        }
    }

    public override void Exit()
    {
        if (attackCoroutine != null)
        {
            MonoBehaviour host = control;
            host.StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        if (control.attackHitboxInstance != null)
        {
            control.attackHitboxInstance.SetActive(false);
        }

        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = false;
        }
    }
}

