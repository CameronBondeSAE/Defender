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
    public bool Charging = false;
    
    // weapon damage - reference beam script
    public int LaserDamage = 5;
    public int charge1Damage = 10;
    public int charge2Damage = 20;
    
   
    
    // Weapon charge
    public float maxCharge = 5.0f;
    public float chargeTime = 0f;
    public void ShotsFired()
    {
        
    }

    public void ShotCharge()
    {
        if (!Charging)
        {
            chargeTime += Time.deltaTime;
            /// how do you say the charge damage do you jst make some if statements
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LBeam.SetActive(false);
        AimBeam.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Use() // laser gun is aquired and is held by the player unil its fired (used)
    {
        AimBeam.SetActive(true);
    }

    public void StopUsing()
    {
        ShotsFired()
    }
    
    

    public void Pickup()
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