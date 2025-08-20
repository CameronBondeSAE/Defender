using Defender;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class FenceBarrier : UsableItem_Base
{
    [Header("Fence Barrier Stuff")]
    [SerializeField] private GameObject prefabFenceObject; // fence prefab

    public override void Use(CharacterBase characterTryingToUse)
    {
        base.Use(characterTryingToUse);

        //Debug.Log("Fence, use");
        Use_Rpc();

    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    private void Use_Rpc()
    {

        Vector3 placePos = transform.position;
        placePos.z += 1.5f;

        //Debug.Log("Location to place: " + placePos);

        Instantiate(prefabFenceObject, placePos, Quaternion.identity); // placement can be quite finicky

        StartCoroutine(destroyDelay());
    }

    IEnumerator destroyDelay()
    {
        yield return new WaitForSeconds(1f);

        //Debug.Log("destroying");
        GetComponent<NetworkObject>().Despawn();
    }
}
