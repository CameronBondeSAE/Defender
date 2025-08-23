using System.Collections;
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
    [SerializeField] float releaseTimer = 30f;
    private bool released = false;

    private void Start()
    {
        SetCollidersEnabled(true);
    }



    public override void Use(CharacterBase characterTryingToUse)
    {
        itemActive = true;
        base.Use(characterTryingToUse);
        BallStart(characterTryingToUse);
    }

    public override void Drop()
    {
        BallStart();
    }


    public void BallStart(CharacterBase characterTryingToUse = null)
    {
        base.Drop();
        //Drop(characterTryingToUse.transform.forward);
        if (characterTryingToUse == null) rb.AddForce(transform.forward * throwForce, ForceMode.Force);
        else rb.AddForce(characterTryingToUse.transform.forward * throwForce, ForceMode.Force);
        if (capturedObject == null)
        {
            Capture(characterTryingToUse);
        }
        else
        {
            Release();
        }

    }

    void Capture(CharacterBase characterTryingToUse)
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
                StartCoroutine(ReleaseTimer());
            }
            else capturedObject = null;
        }
    }

    void Release()
    {
        released = true;
        capturedObject.transform.parent = null;
        capturedObject.SetActive(true);
        capturedObject.transform.position = transform.position;
        capturedObject = null;
        GetComponent<NetworkObject>().Despawn();
    }


    IEnumerator ReleaseTimer()
    {
        yield return new WaitForSeconds(releaseTimer);
        if (!released) Release();
    }


}
