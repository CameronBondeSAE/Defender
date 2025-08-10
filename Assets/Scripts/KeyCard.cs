using System;
using Defender;
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
            inUse = true;
            Debug.Log("KeyCard used");
            GetComponentInChildren<MeshRenderer>().material =  keycardUsedMat;
		
            if (activationCountdown > 0)
                StartActivationCountdown_LocalUI(Mathf.CeilToInt(activationCountdown));
        }
    }
    
    protected override void ActivateItem()
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

    public override void Pickup()
    {
        base.Pickup(); // Plays pickup sound, etc
    }

    public override void Drop()
    {
        base.Drop(); // Plays drop sound, etc
    }
}
