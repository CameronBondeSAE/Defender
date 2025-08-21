using System.Collections;
using Defender;
using Unity.Mathematics.Geometry;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Laser_Gun_Script : UsableItem_Base
{
    // game object declarations 
    public GameObject LBeam;
    public GameObject Oneshot_Laser;
    public GameObject AimBeam;

    public bool isCharging = false;

    public LaserBeam laserBeam;
    public PlayerInventory playerInventory;
    

    // Weapon charge
    public float maxCharge = 7.0f;
    public float chargeTime = 0f;

    // instatiation for the beam //
    private void BeamFire()
    {
        Instantiate(LBeam, transform.position, transform.rotation);
    }

    private void ScaleBeam(GameObject LBeam) // scales an instance of the laser beam 
    {
        if (!isCharging)
            LBeam.transform.localScale = Vector3.one * Mathf.Lerp(1f, 3f, Mathf.Clamp01(chargeTime / maxCharge));
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

    public void Use(CharacterBase characterTryingToUse) // laser gun is aquired and is held by the player unil its fired checks for charge or single shot (used)
    {
        if (!isCharging)
        {
            StartCoroutine(ShotCharge());
        }
    }

    IEnumerator ShotCharge()
    {
        while (isCharging)
        {
            chargeTime += Time.deltaTime;
            yield return null;
        }
        
    }




    public void StopUsing() // weapon has been fired then is destoryed 
    {
        if (LBeam != null && isCharging == false) // if a laser beam has been fired
        {
            if (chargeTime < 2f)
            {
                ScaleBeam(LBeam);
                laserBeam.LaserDamage = 5;
                BeamFire();
            }

            else if (chargeTime > 4.5f)
            {
                ScaleBeam(LBeam);
                laserBeam.LaserDamage = 10;
                BeamFire();
            }

            else if (chargeTime > 5.2f)
            {
                ScaleBeam(LBeam);
                laserBeam.LaserDamage = 15;
                BeamFire();
            }
        }
    }

    public void Pickup(CharacterBase whoIsPickupMeUp)
        {
         
            AimBeam.SetActive(true);
        }

        public void Drop()
        {
            AimBeam.SetActive(false);
            Destroy(Oneshot_Laser);
        }
        
}