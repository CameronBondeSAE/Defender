using Defender;
using UnityEngine;
/// Use this interface for items that can be used directly (potions, bombs, etc.)
public interface IUsable
{
    void Use(CharacterBase characterTryingToUse);
    void StopUsing();
}

/// EXAMPLE INTERFACE for laser-like persistent items - things that can be used for a duration of time
// public interface ILaserItem
// {
//     void ActivateLaser();
//     void DeactivateLaser();
// }
