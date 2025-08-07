using UnityEngine;

public class OneShot_Laser : MonoBehaviour, IUsable
{
    // Variable declarations ///
    public GameObject LBeam;
    public GameObject Oneshot_Laser;
    public GameObject AimBeam;
    public bool isGrabbed = false; // detects if a weapon has been picked up 
    public bool gunFired = false; // this checks for when the gun on hand has been fired 
    
    // Update is called once per frame
    void Update()
    {
        
    }

    public void Use() // laser is aquired // what do i write when the player grabs the laser gun? //
    {
        if (Input.GetMouseButtonDown(0) && isGrabbed) // player left clicks 
        {
            LBeam.SetActive(true);
            // spawns the beam directly can this be reused for the laser// Instantiate(LBeam, transform.position, Quaternion.identity);
            gunFired = true;
        }
      // wait for inpuy  else (isGrabbed)
        {
            
        }

        {
            if (AimBeam != null)
            {
                AimBeam.SetActive(true);
            }
        }
    }

    public void StopUsing() // after laser has been fired weapon on hand is destroyed
    {
        if (gunFired == true)
        {
            Destroy(OneShot_Laser);
        }
    }





    // void EmptyWeapon() // reusable function that destroyed the weapon once it has been fired

}

