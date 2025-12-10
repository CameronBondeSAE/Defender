using Defender;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
public class smartAlienViewer : NetworkBehaviour
{
    public SmartAlienSfx alienSfx;
    public AudioSource Source;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    
    public void DestoryAlienSfx_RPC()
    {
     alienSfx.audioSource.Play();
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    
    public void escortStart_RPC()
    {
     alienSfx.audioSource.Play();   
    }
    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    
    public void playThreateDisabeld_RPC()
    {
        alienSfx.audioSource.Play();
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]

    public void DroppedCiv_RPC()
    {
     alienSfx.audioSource.Play();   
    }
}
