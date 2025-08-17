using Defender;
using UnityEngine;

public class BlowingFanScript : UsableItem_Base
{ 
    //Push Control
    void OnTriggerStay(Collider other)
    {
        Rigidbody otherRigidbody = other.GetComponent<Rigidbody>();
        if (otherRigidbody != null)
        {
            Vector3 pushDirection = (other.transform.position - transform.position).normalized;
            otherRigidbody.AddForce(pushDirection * pushForce, ForceMode.Acceleration);
        }
    }
    
    //Particle Control
    [SerializeField] private float pushForce;
    public ParticleSystem particleSystem;

    public void Use(CharacterBase characterTryingToUse)
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
