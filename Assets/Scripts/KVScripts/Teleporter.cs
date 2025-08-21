using System;
using Defender;
using UnityEngine;
using Unity.Netcode;

public class Teleporter : UsableItem_Base
{
    [Header("Teleporter Settings")]
    public Transform destination; // The exit teleporter
    public AudioClip teleportClip;
    public float audioHearingRadius = 15f;
    
    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody otherRigidbody = other.GetComponent<Rigidbody>();
        if (otherRigidbody != null)
        {
            otherRigidbody.transform.position = destination.position;
        }
    }
    
    public override void Use(CharacterBase characterTryingToUse)
    {
        throw new System.NotImplementedException(); 
    }

    public void StopUsing()
    {
        throw new System.NotImplementedException();
    }

    public void Pickup(CharacterBase whoIsPickupMeUp)
    {
        throw new System.NotImplementedException();
    }

    public void Drop()
    {
        throw new System.NotImplementedException();
    }
}
