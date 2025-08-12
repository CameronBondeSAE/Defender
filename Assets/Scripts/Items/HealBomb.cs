using UnityEngine;
using Defender;
using Unity.Netcode;
using System.Diagnostics;

public class HealBomb : UsableItem_Base
{
    private bool itemActive = false;
    [SerializeField] float effectRadius = 5f;
    [SerializeField] float healingAmount = 10f;

    private void Start()
    {
        SetCollidersEnabled(true);
    }



    public override void Use(CharacterBase characterTryingToUse)
    {
        itemActive = true;
        base.Use(characterTryingToUse);
        Drop();

    }

    public override void Drop()
    {
        base.Drop();
        if (itemActive)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, effectRadius);
            foreach (var hitCollider in hitColliders)
            {
                Health health = hitCollider.GetComponent<Health>();
                if (health != null)
                {
                    health.Heal(healingAmount);
                }
            }
            GetComponent<NetworkObject>().Despawn();
        }

    }
}
