using System;
using Defender;
using UnityEngine;

public class BlowingFanScript : UsableItem_Base
{
    private void Start()
    {
        particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public bool fanOn;
    
    //Push Control
    public void OnTriggerStay(Collider other)
    {
        
        Rigidbody otherRigidbody = other.GetComponent<Rigidbody>();
        if (fanOn == true && otherRigidbody != null)
        {
            Vector3 pushDirection = (other.transform.position - transform.position).normalized;
            otherRigidbody.AddForce(pushDirection * pushForce, ForceMode.Acceleration);
        }
    }
    
    //Particle Control
    [SerializeField] private float pushForce;
    public ParticleSystem particleSystem;

    public override void Use(CharacterBase characterTryingToUse)
    {
        particleSystem.Play();
        fanOn = true;
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
