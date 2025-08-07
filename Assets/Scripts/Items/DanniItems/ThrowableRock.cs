using UnityEngine;


public class ThrowableRock : UsableItem_Base
{
    public override void Use()
    {
        Launch(transform.forward, 15f);
    }
}
