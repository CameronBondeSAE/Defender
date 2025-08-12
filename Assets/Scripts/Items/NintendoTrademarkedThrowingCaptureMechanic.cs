using Defender;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NintendoTrademarkedThrowingCaptureMechanic : UsableItem_Base
{
    private bool itemActive = false;
    [SerializeField] float effectRadius = 5f;
    [SerializeField] float healingAmount = 10f;
    [SerializeField] GameObject capturedObject = null;
    [SerializeField] float throwForce = 4f;

    private void Start()
    {
        SetCollidersEnabled(true);
    }



    public override void Use(CharacterBase characterTryingToUse)
    {
        itemActive = true;
        base.Use(characterTryingToUse);
        Capture(characterTryingToUse);

    }

    public void Capture(CharacterBase characterTryingToUse)
    {
        Drop();
        Launch(characterTryingToUse.transform.forward, throwForce);
        if (capturedObject == null)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, effectRadius);
            List<Health> healths = new List<Health>();
            foreach (var hitCollider in hitColliders)
            {
                Health health = hitCollider.GetComponent<Health>();
                if (health != null)
                {
                    healths.Add(health);
                }
            }

            if (healths.Count > 0)
            {
                capturedObject = healths[Random.Range(0, healths.Count)].gameObject;
                if (characterTryingToUse.gameObject != capturedObject)
                {
                    capturedObject.transform.parent = transform;
                    capturedObject.SetActive(false);
                }
                else capturedObject = null;
            }

        }
        else
        {
            capturedObject.transform.parent = null;
            capturedObject.SetActive(true);
            capturedObject.transform.position = transform.position;
            capturedObject = null;
            GetComponent<NetworkObject>().Despawn();
        }

    }
}
