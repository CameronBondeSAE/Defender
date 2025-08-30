using Defender;
using Unity.Netcode;
using UnityEngine;

public class KeyCard : UsableItem_Base
{
    [Header("Keycard Settings")]
    public string KeyId;
    public Material keycardUsedMat;
    public float useTime;
    public GameObject currentGate;

    public bool inUse;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    protected override void Awake()
    {
        base.Awake();
        activationCountdown = useTime;
    }
    public override void Use(CharacterBase characterTryingToUse)
    {
        if (currentGate != null && !inUse)
        {
            base.Use(characterTryingToUse);
		
            if (activationCountdown > 0)
                StartActivationCountdown_LocalUI(Mathf.CeilToInt(activationCountdown));
            
            UseClient_Rpc();
        }
    }
    
    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    private void UseClient_Rpc()
    {
        inUse = true;
        Debug.Log("KeyCard used");
        GetComponentInChildren<MeshRenderer>().material =  keycardUsedMat;
    }
    
    protected override void ActivateItem()
    {
        base.ActivateItem();
		
        ActivateItemClient_Rpc();
    }
    
    [Rpc(SendTo.ClientsAndHost, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
    private void ActivateItemClient_Rpc()
    {
        Debug.Log("DemoItem ACTIVATED!");
        GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
        currentGate.GetComponent<SlidingDoor>().isOpen = true;
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<SlidingDoor>() != null)
        {
            currentGate = other.gameObject;
        }
    }


    public override void Drop()
    {
        base.Drop(); // Plays drop sound, etc
    }
}
