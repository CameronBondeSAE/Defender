using UnityEngine;

public class DestructibleWall : Health
{
    protected override void Die()
    {
        base.Die();
        Destroy(gameObject, 0.5f);
    }
}
