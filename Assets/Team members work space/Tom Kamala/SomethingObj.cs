using Defender;
using Unity.Netcode;
using UnityEngine;

public class SomethingObj : NetworkBehaviour, IUsable
{
    public void PickUp(CharacterBase characterTryingToPickUp)
    {
        throw new System.NotImplementedException();
    }
    public void Use(CharacterBase characterTryingToUse)
    {
        throw new System.NotImplementedException();
    }

    public void StopUsing()
    {
        throw new System.NotImplementedException();
    }
}
