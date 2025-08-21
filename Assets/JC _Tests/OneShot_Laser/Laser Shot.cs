using System;
using System.Collections;
using Defender;
using UnityEngine;
using UnityEngine.InputSystem;

public class LaserShot : UsableItem_Base
{
    // game object declarations 
    public GameObject LBeam;
    public GameObject Oneshot_Laser;
    public GameObject AimBeam;

    public float chargeTime = 0f;
    public float maxCharge = 7f;
    
    public bool isCharging = false;
    
    public LaserBeam laserBeam; // laser beam script reference
    
    private void BeamFire()
    {
        Instantiate(LBeam, transform.position, transform.rotation);
    }

    private void ScaleBeam(GameObject LBeam) // scales an instance of the laser beam 
    {
        if (!isCharging)
            LBeam.transform.localScale = Vector3.one * Mathf.Lerp(1f, 3f, Mathf.Clamp01(chargeTime / maxCharge));
    }

    
    void Start()
    {
        AimBeam.SetActive(false);
    }
    

    void ShotCharge()
    {
        if (!isCharging)
        {
           isCharging = true;
            StartCoroutine(LaserCharge());
            GameObject beam = Instantiate(LBeam, transform.position, transform.rotation);
        }
    }

    IEnumerator LaserCharge()
    {
        chargeTime += Time.deltaTime;
        if (chargeTime > 2f)
        {
            Debug.Log("LaserCharge 1"); // try add audio 
        }
        else if (chargeTime > 4f)
        {
            Debug.Log("Laser charge 2");
        }
        else if (chargeTime > 6f)
        {
            Debug.Log("Max Charge");
        }

        yield return new WaitForSeconds(chargeTime);
    }
    
    public void Use(CharacterBase characterTryingToUse)
    {
        if (isCharging)
        {
            ShotCharge();
            LBeam.transform.localScale = Vector3.one * Mathf.Lerp(1f, 3f, Mathf.Clamp01(chargeTime / maxCharge));
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
            chargeTime = 0f;
        }
    }

    public override void Pickup(CharacterBase whoIsPickupMeUp)
    {
        base.Pickup(whoIsPickupMeUp); // plays audio, sets IsCarried, disables physics 
        AimBeam.SetActive(true);
    }

    public override void Drop()
    {
        throw new System.NotImplementedException();
    }
}
