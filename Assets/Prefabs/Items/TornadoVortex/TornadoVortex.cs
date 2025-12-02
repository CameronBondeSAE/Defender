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
    [SerializeField] private List<Rigidbody> nameOfObjectsNearItem;

    [SerializeField]private float strength = 10f;


    public void FixedUpdate()
    {
        //Todo add a bool
        foreach (Rigidbody item in nameOfObjectsNearItem)
        {
            if (item != null)
            {
                //todo bool for repel or attract
                item.AddForce((transform.position - item.position).normalized * strength);
                
                //rbObjectsNearItem.AddForce(Vector3.up * strength, ForceMode.Acceleration);
                //rbObjectsNearItem.AddTorque(Vector3.up);
            }
        }
    }
    public override void Use(CharacterBase characterTryingToUse)
    {
        base.Use(characterTryingToUse);

        // toggle for on or off
        
        Debug.Log("Use TornadoItem : By "+characterTryingToUse.name);
    }
    public override void Pickup(CharacterBase whoIsPickupMeUp)
    {
        base.Pickup(whoIsPickupMeUp);
		
        Debug.Log("Tornado Item picked up by : "+whoIsPickupMeUp.name);
    }

    public void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb)
        {
            if (!nameOfObjectsNearItem.Contains(rb))
            {
                nameOfObjectsNearItem.Add(rb);
                Debug.Log("Item is now at " + nameOfObjectsNearItem.Count);
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Rigidbody>())
        {
            nameOfObjectsNearItem.Remove(other.GetComponent<Rigidbody>());
        }
    }
}
