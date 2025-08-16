using Defender;
using UnityEngine;

public class EMPGrenade : MonoBehaviour, IUsable, IPickup
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
    // variable declarations //
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Use(CharacterBase characterTryingToUse)
    {
        throw new System.NotImplementedException(); // if player input
        
    }

    public void StopUsing()
    {
        throw new System.NotImplementedException();
    }

    public void Pickup(CharacterBase whoIsPickupMeUp)
    {
        throw new System.NotImplementedException(); // turn the grenade green
    }

    public void Drop()
    {
        throw new System.NotImplementedException();
    }
}

// the goal for the emp grenade is to disable the motherships and stub aliens