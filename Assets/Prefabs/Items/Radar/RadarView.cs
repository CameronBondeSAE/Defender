using UnityEngine;
using System.Collections;
using Defender;
using NUnit.Framework;
using Unity.Netcode;
public class RadarView : UsableItem_Base
{
    public RadarRays radar;
    public AudioSource radarAudio;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]

    public void RadarSound_RPC()
    {
     radarAudio.Play();
    }
}
