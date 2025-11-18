using Defender;
using UnityEngine;

public class SlimeGrenade : UsableItem_Base
{
    [Space]
    [Header("Slime Grenade Settings")]
    [SerializeField]public ParticleSystem slimeParticle;
    [SerializeField] public GameObject slimeField;

    void Start()
    {
        slimeParticle.gameObject.SetActive(false);
        if (slimeField != null)
        {
            slimeField.SetActive(false);
        }   
    }
    public override void Pickup(CharacterBase whoIsPickupMeUp)
    {
        base.Pickup(whoIsPickupMeUp);
        
        Debug.Log("SlimeGrenade picked up");
    }

    public override void Use(CharacterBase characterTryingToUse)
    {
        base.Use(characterTryingToUse);
        
        // Activate particle system
        slimeParticle.gameObject.SetActive(true);
        slimeParticle.Play();
        
        if (slimeField != null)
        {
            slimeField.SetActive(true);
        }
        
        Debug.Log("SlimeGrenade use by " + characterTryingToUse.name);
    }

    public override void Drop()
    {
        base.Drop();
        
        // Reset rotation to zero
        transform.rotation = Quaternion.identity;
    }
    
    public override void Drop(Vector3 dropPosition)
    {
        base.Drop(dropPosition);
        
        // Reset rotation to zero
        transform.rotation = Quaternion.identity; // This sets rotation to (0,0,0)
    }
}
