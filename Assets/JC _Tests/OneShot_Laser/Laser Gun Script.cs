using System.Collections;
using Defender;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Laser_Gun_Script : MonoBehaviour, IUsable, IPickup
{
    // game object declarations 
    public GameObject LBeam;
    public GameObject Oneshot_Laser;
    public GameObject AimBeam;
    public bool isGrabbed = false; // detects if a weapon has been picked up 
    public bool gunFired = false; // this checks for when the gun on hand has been fired 
    public bool isCharging = false;

    public LaserBeam laserBeam;

    // Weapon charge
    public float maxCharge = 7.0f;
    public float chargeTime = 0f;

    public void ShotsFired()
    {
        if (InputSystem.GetDevice<Mouse>().leftButton.isPressed)
        {
            
        }
        else if (InputSystem.GetDevice<Mouse>().leftButton.isPressed)
        {
            /// instatiate other objects
        }
    }

    public void ShotCharge()
    {
        if (!isCharging) // cold resolve an issue uising the bool here
        {
            StartCoroutine(ChargingShot());
        }
    }

    IEnumerator ChargingShot()
    {
        if (chargeTime <= 4.0f) // fires single shot dealing 5 damage //
        {
            Instantiate(LBeam, transform.position, transform.rotation);
        }

        if (chargeTime >= 4f) // 10 damage
        {
        }

        if (chargeTime == maxCharge) // 15 damage
        {
            //
        }

        yield return null;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LBeam.SetActive(false);
        AimBeam.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void Use(CharacterBase characterTryingToUse) // laser gun is aquired and is held by the player unil its fired (used)
    {
        AimBeam.SetActive(true);
    }


    public void StopUsing()
    {
        // if
        //ShotsFired()
    }

    public void Pickup(CharacterBase whoIsPickupMeUp)
    { 
        AimBeam.SetActive(true);
    }

    public void Drop()
    {
        throw new System.NotImplementedException();
    }
}


// single shot code if (InputSystem.GetDevice<Mouse>().leftButton.wasPressedThisFrame)
// charged shot code