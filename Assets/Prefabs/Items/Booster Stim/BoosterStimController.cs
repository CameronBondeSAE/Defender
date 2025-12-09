using UnityEngine;
using System.Collections;
using Defender;
using NUnit.Framework;
using Unity.Netcode;
public class BoosterStimController : UsableItem_Base
{
    public BoosterStim StimModel;
    public StimView StimView;
    [SerializeField] private KeyCode SpaceBar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(SpaceBar))
        // {
        //  StimModel.ActivateBoosterStim_Rpc();
        //  StimView.StimSound_RPC();
        //  StimView.StimParticle_RPC();
        // }     
       
    }
}
