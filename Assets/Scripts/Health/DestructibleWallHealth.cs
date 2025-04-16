public class DestructibleWallHealth : Health
{
    protected override void Die()
    {
        base.Die();
        Destroy(gameObject, 0.5f);
    }
}