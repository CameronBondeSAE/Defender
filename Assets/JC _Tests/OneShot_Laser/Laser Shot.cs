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

    void ShotCharge()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            isCharging = true;
            StartCoroutine(LaserCharge());
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

        yield return null;
    }

    private void FireLaser()
    {
        GameObject beamInstance = Instantiate(LBeam, transform.position, transform.rotation);
        float t = chargeTime;
    }

    public override void Use(CharacterBase characterTryingToUse)
    {
        if (isCharging)
        {
            ShotCharge();
            LBeam.transform.localScale = Vector3.one * Mathf.Lerp(1f, 3f, Mathf.Clamp01(chargeTime / maxCharge));
        }
    }

    public override void StopUsing()
    {
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isCharging = false;
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
