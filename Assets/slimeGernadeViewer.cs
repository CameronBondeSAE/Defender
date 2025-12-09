using UnityEngine;
using System.Collections;
using Defender;
using NUnit.Framework;
using Unity.Netcode;
public class slimeGernadeViewer : UsableItem_Base
{
    public SlimeGrenade SG;

    public AudioSource slimeImpact;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]

    public void Active_RPC()
    {
     SG.slimeField.active = true;
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]

    public void SlimeSound_RPC()
    {
    slimeImpact.Play();    
    }
        
}
