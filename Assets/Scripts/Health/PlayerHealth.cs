using System.Collections;
using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] private float reviveAnimDuration;

    protected override void Die()
    {
        base.Die();
        //PlayerEventManager.instance.events.onDeath.Invoke();
        StartCoroutine(RespawnPlayer());
        // respawn code
    }

    private IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(deathAnimDuration);
        yield return new WaitForSeconds(reviveAnimDuration);
        currentHealth = maxHealth;
        base.Revive();
        

        //PlayerEventManager.instance.events.onIdle.Invoke();
        transform.position = new Vector3(0, 1, 0); // change to spawn position when that's been decided
    }
}