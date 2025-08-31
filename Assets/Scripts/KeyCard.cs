using System;
using Defender;
using Unity.Netcode;
using UnityEngine;

public class KeyCard : UsableItem_Base
{
    [Header("Keycard Settings")]
    //public string KeyId; Deprecated feature, beg (just ask) Jai to implement
    public Material keycardUsedMat;
    public float useTime;
    public GameObject currentGate;

    public bool inUse;
    
    protected override void Awake()
    {
        base.Awake();
        //sets generic activationCountdown to variable useTime
        activationCountdown = useTime;
    }
    //Override generic Use
    public override void Use(CharacterBase characterTryingToUse)
    {
        //if there is a gate in proximity and the keycard isnt already being used
        if (currentGate != null && !inUse)
        {
            //generic Use function
            base.Use(characterTryingToUse);
		
            //generics for the timer visual
            if (activationCountdown > 0)
                StartActivationCountdown_LocalUI(Mathf.CeilToInt(activationCountdown));
            
            //UseClient Rpc Function
            UseClient_Rpc();
        }
    }
    
    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    private void UseClient_Rpc()
    {
        //Set keycard to being in use
        inUse = true;
        Debug.Log("KeyCard used");
        //change to in use material
        GetComponentInChildren<MeshRenderer>().material =  keycardUsedMat;
        //material was used instead of just altering the colour so that in future the keycard could have custom textured materials for not used and used
    }
    
    protected override void ActivateItem()
    {
        //Generic ActivateItem
        base.ActivateItem();
		
        //ActivateItemClient Rpc function
        ActivateItemClient_Rpc();
    }
    
    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    private void ActivateItemClient_Rpc()
    {
        Debug.Log("DemoItem ACTIVATED!");
        //tell the gate its open
        currentGate.GetComponent<SlidingDoor>().isOpen = true;
        //destroy self
        Destroy(gameObject);
    }

    //on enter trigger
    private void OnTriggerEnter(Collider other)
    {
        //if its a door
        if (other.GetComponent<SlidingDoor>() != null)
        {
            //set current gate to the triggered object
            currentGate = other.gameObject;
        }
    }

    //on exit trigger
    private void OnTriggerExit(Collider other)
    {
        //if the object left is the current gate
        if (other.gameObject == currentGate)
        {
            //set current gate to null
            currentGate = null;
        }
    }
}
