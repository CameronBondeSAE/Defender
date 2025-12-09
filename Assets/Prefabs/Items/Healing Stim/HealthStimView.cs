using Unity.Netcode;
using UnityEngine;

public class HealthStimView : UsableItem_Base
{
    public healthStimController healthStimController;
    public AudioSource StimSource;
    public HealingStim healthStim;
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
     StimSource.Play();   
    }

}
