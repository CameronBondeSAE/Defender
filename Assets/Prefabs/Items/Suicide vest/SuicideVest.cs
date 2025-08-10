using Defender;
using System.Buffers;
using UnityEngine;

/// <summary>
/// on pickup, the vest is not activated.
/// oncollision enter, attach to the other character (not the owner)
/// </summary>
public class SuicideVest : UsableItem_Base
{
    [Space]
    [Header("Vest Settings")]
    [SerializeField] private Transform vestTransform;
    [SerializeField] private CharacterBase owner;
    private enum VestState { disabled, inHand, isAttached };
    [SerializeField] private VestState state;

    public override void StopUsing()
    {
        base.StopUsing();

        state = VestState.disabled;

        Debug.Log("Vest disabled");
    }

    public override void Use(CharacterBase characterTryingToUse)
    {
        base.Use(characterTryingToUse);

        Drop();
        Launch(transform.forward, 50f); // TODO get actual forward direction

        // on pickup, the vest is not activated. instead it is assigned an owner
        // on collision enter, attach the vest to the other character (whether that be a civ, alien, or player)
    }

    private void GetHolderFacingDirection()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        //vestTransform.SetParent(gameObject.transform);

        if (state == VestState.inHand)
        {

        }
    }

    private void Explode()
    {
        // once attached, explode after a set timer

        if (state == VestState.isAttached)
        {

        }
    }
}
