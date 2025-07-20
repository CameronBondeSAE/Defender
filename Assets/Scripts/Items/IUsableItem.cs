using UnityEngine;
/// Use this interface for items that can be used directly (potions, bombs, etc.)
public interface IUsableItem
{
    void UseItem();
}

/// EXAMPLE INTERFACE for laser-like persistent items - things that can be used for a duration of time
// public interface ILaserItem
// {
//     void ActivateLaser();
//     void DeactivateLaser();
// }
