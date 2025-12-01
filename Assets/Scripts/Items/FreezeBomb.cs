using Defender;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FreezeBomb : UsableItem_Base
{
    private bool itemActive = false;
    [SerializeField] float effectRadius = 5f;
    [SerializeField] float healingAmount = 10f;
    [SerializeField] float throwForce = 4f;
    [SerializeField] float frozenTime = 10f;

    private void Start()
    {
        SetCollidersEnabled(true);
    }



    public override void Use(CharacterBase characterTryingToUse)
    {
        itemActive = true;
        base.Use(characterTryingToUse);
        // Freeze(characterTryingToUse);
    }

    public override void Drop()
    {
        // Freeze();
    }


    public void Freeze(CharacterBase characterTryingToUse = null)
    {
        base.Drop();
        //Drop(characterTryingToUse.transform.forward);
        if (characterTryingToUse == null) rb.AddForce(transform.forward * throwForce, ForceMode.Force);
        else rb.AddForce(characterTryingToUse.transform.forward * throwForce, ForceMode.Force);
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, effectRadius);
        foreach (var hitCollider in hitColliders)
        {
            Health health = hitCollider.GetComponent<Health>();
            if (health != null)
            {
                health.gameObject.AddComponent<FrozenAwait>();
                FrozenAwait frozenAwait = health.GetComponent<FrozenAwait>();
                StartCoroutine(frozenAwait.FunFreezeWait(frozenTime));
            }
        }
        GetComponent<NetworkObject>().Despawn();

    }
}
