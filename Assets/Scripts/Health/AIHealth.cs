using UnityEngine;

public class AIHealth : Health
{
    protected override void Die()
    {
        base.Die();
        // plays animation
        Destroy(gameObject, deathAnimDuration);
    }

    public void Kill()
    {
        base.Die();
        Debug.Log(gameObject.name + " is killed");
        Destroy(gameObject);
    }
}