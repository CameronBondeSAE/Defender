using UnityEngine;
using System.Collections;
using Defender;
using NUnit.Framework;
using Unity.Netcode;

public class StimView : UsableItem_Base
{
    public BoosterStimController stimController;
    public BoosterStim stim;
    public ParticleSystem stimParticle;
    public AudioSource stimSound;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]

    public void StimSound_RPC()
    {
     stimSound.Play();   
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]

    public void StimParticle_RPC()
    {
     stimParticle.Play();
     stimParticle.loop = true;
    }
}
