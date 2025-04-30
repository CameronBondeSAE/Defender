public class AIHealth : Health
{
    protected override void Die()
    {
        base.Die();
        // plays animation
        Destroy(gameObject, deathAnimDuration);
    }
}