using System;
using System.Collections.Generic;
using Defender;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class TornadoVortex : UsableItem_Base
{
    [Header("Tornado Vortex")]
    [SerializeField] private List<Rigidbody> nameOfObjectsNearItem;

    [SerializeField]private float strength = 200f;

    [SerializeField] private GameObject plasmaSphere;
    private bool isActivated = false;
    private bool toggleItem = false;


    public void FixedUpdate()
    {
        if (isActivated)
        {
            foreach (Rigidbody item in nameOfObjectsNearItem)
            {
                if (item != null)
                {
                    if (toggleItem)
                    {
                        item.AddForce((transform.position - item.position).normalized * strength);
                    }
                    else
                    {
                        item.AddForce((item.position - transform.position).normalized * strength);
                    }
                }
            }
            
        }
        
    }
    public override void Use(CharacterBase characterTryingToUse)
    {
        base.Use(characterTryingToUse);
        isActivated = true;
        toggleItem = !toggleItem;
        
        //plasmaSphere.SetActive(false);
        
        plasmaSphere.transform.DOScale(new Vector3(6f,6f,6f), 5f).SetEase(Ease.InCubic);
        // toggle for on or off
        
        Debug.Log("Use TornadoItem : By "+characterTryingToUse.name);
    }

    public override void StopUsing()
    {
        base.StopUsing();
        isActivated = false;
        plasmaSphere.transform.DOScale(new Vector3(1f,1f,1f), 5f).SetEase(Ease.InCubic);
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
