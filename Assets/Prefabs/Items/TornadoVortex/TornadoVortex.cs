using System;
using System.Collections.Generic;
using Defender;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class TornadoVortex : UsableItem_Base
{
    [Header("Tornado Vortex")]
    [SerializeField] private List<GameObject> nameOfObjectsNearItem;
    private Rigidbody _rigidbody;

    public void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public override void Use(CharacterBase characterTryingToUse)
    {
        base.Use(characterTryingToUse);
		
        Debug.Log("Use GetTiny_Model : By "+characterTryingToUse.name);
        
    }
    public override void Pickup(CharacterBase whoIsPickupMeUp)
    {
        base.Pickup(whoIsPickupMeUp);
		
        Debug.Log("Tornado Item picked up by : "+whoIsPickupMeUp.name);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (_rigidbody != null)
        {
            other.gameObject.GetComponent<Rigidbody>();
            nameOfObjectsNearItem.Add(other.gameObject);
            Debug.Log("Item is now at " + nameOfObjectsNearItem.Count);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (_rigidbody != null)
        {
            other.gameObject.GetComponent<Rigidbody>();
            nameOfObjectsNearItem.Remove(other.gameObject);
        }
    }
}
