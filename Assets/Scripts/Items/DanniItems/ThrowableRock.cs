using Defender;
using UnityEngine;


public class ThrowableRock : UsableItem_Base
{
    public override void Use(CharacterBase characterTryingToUse)
    {
        Launch(transform.forward, 15f);
    }
}
