using System.Collections;
using UnityEngine;
using AIAnimation;

public class PlayerHealth : Health
{
    [SerializeField] private float reviveAnimDuration;
    private AIAnimationController animation;

    protected override void Die()
    {
        base.Die();
        //PlayerEventManager.instance.events.onDeath.Invoke();
        StartCoroutine(RespawnPlayer());
        // respawn code
    }

    private IEnumerator RespawnPlayer()
    {
        animation = GetComponent<AIAnimationController>();
        animation.SetAnimation(AIAnimationController.AnimationState.Dead);
        yield return new WaitForSeconds(deathAnimDuration);
        yield return new WaitForSeconds(reviveAnimDuration);
        currentHealth.Value = maxHealth;
        base.Revive();
        

        //PlayerEventManager.instance.events.onIdle.Invoke();
        transform.position = new Vector3(0, 1, 0); // change to spawn position when that's been decided
    }
}