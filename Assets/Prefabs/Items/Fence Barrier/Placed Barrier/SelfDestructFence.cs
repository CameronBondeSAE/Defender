using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class SelfDestructFence : NetworkBehaviour
{

    [SerializeField] private float destructionTime = 10f;
    [SerializeField] private Rigidbody rbFence;



    private void OnEnable()
    {
        NetworkObject.Spawn(this);
        StartCoroutine(WaitForSeconds());
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log("networkspawn fence");
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
    private void StartTimer_Rpc()
    {
        StartCoroutine(SelfDestruct());
    }

    IEnumerator SelfDestruct()
    {
        rbFence = GetComponent<Rigidbody>();
        rbFence.isKinematic = false;

        Debug.Log("selfdestruct timer");
        yield return new WaitForSeconds(destructionTime);
        DestroyOnTimer_Rpc();

    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = false)] // , RequireOwnership = true
    private void DestroyOnTimer_Rpc()
    {
        Debug.Log("destroy fence");
        Destroy(gameObject);
    }

    IEnumerator WaitForSeconds()
    {
        yield return new WaitForSeconds(1);
        Debug.Log("waitforseconds");
        StartTimer_Rpc();
    }

}
