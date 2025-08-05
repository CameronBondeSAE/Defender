using UnityEngine;


public class ThrowableRock : UsableItem
{
    public override void Use()
    {
        Launch(transform.forward, 15f);
    }
}
