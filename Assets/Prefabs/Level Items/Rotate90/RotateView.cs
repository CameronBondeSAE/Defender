using UnityEngine;
using System.Collections;
using Defender;
using NUnit.Framework;
using Unity.Netcode;
public class RotateView : UsableItem_Base
{
    public Rotate90_Model stoneMove;
    public ParticleSystem stoneEmiterRight;
    public ParticleSystem stoneEmiterLeft;
    public AudioSource stoneSound;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    public void stoneSound_RPC()
    {
     stoneSound.Play();    
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    public void stoneEmitRight_RPC()
    {
    stoneEmiterRight.Play();
    StartCoroutine(EmitRoutine());
    }

    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    public void stoneEmitLeft_RPC()
    {
     stoneEmiterLeft.Play();
     StartCoroutine(EmitRoutine());
    }
    
    private IEnumerator EmitRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        {
        stoneEmiterRight.Stop();    
        }
    }
}
