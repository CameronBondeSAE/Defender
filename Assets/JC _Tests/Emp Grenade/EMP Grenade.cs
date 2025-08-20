using Defender;
using UnityEngine;

public class EMPGrenade : UsableItem_Base 
{
    public float empRadius = 5f;
    private float empCountdown = 5f;


    private void PowerOut()
    {
        base.Awake();
        activationCountdown = empCountdown;
    }

    public override void Use(CharacterBase characterTryingToUse)
    {
        // Launch itself forward (when used)
        if (characterTryingToUse != null)
        {
            Debug.Log("Grenade thrown!");
            Launch(characterTryingToUse.transform.forward, launchForce);
            base.Use(characterTryingToUse); // starts the activation countdown
        }
    }

    public override void Pickup(CharacterBase whoIsPickupMeUp)
    {
        base.Pickup(whoIsPickupMeUp); // plays audio, sets IsCarried, disables physics
        // detect and store the carrier
        Debug.Log("EMP Acquired");
    }

    protected override void ActivateItem() // activates rhe grenade and activates the alien check
    {
        AlienCheck();
    }
    private void AlienCheck() // checks for aliens 
    {
        Collider[] Alienrange = Physics.OverlapSphere(transform.position, empRadius);
       
         foreach (Collider c in Alienrange)
         {
             if (c.CompareTag("Alien"))
             {
                 Debug.Log("EMP Activated");
                 c.GetComponent<Rigidbody>().isKinematic = true;
                 PowerOut();
             }
         }
    }
    
}
