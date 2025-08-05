using System.Buffers;
using UnityEngine;

/// <summary>
/// on pickup, the vest is not activated.
/// oncollision enter, attach to the other character (not the owner)
/// </summary>
public class SuicideVest : MonoBehaviour, IUsable
{
    [SerializeField] private Transform vestTransform;
    [SerializeField] private GameObject owner;
    private enum VestState { disabled, inHand, isAttached };
    [SerializeField] private VestState state;

    public void StopUsing()
    {
        state = VestState.disabled;

        Debug.Log("Vest disabled");
    }

    public void Use()
    {
        // on pickup, the vest is not activated. instead it is assigned an owner
        // on collision enter, attach the vest to the other character (whether that be a civ, alien, or player)
    }

    private void OnTriggerEnter(Collider other)
    {
        //vestTransform.SetParent(gameObject.transform);
    }
}
