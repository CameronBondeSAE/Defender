using System;
using Defender;
using UnityEngine;

public class RadarRays : UsableItem_Base
{
    public float speed     = 5f;
    public bool  activated = false;

    public override void Use(CharacterBase characterTryingToUse)
    {
        base.Use(characterTryingToUse);

        activated = !activated;
    }
    public override void Pickup(CharacterBase whoIsPickupMeUp)
    {
        base.Pickup(whoIsPickupMeUp);
		
        Debug.Log("Tornado Item picked up by : "+whoIsPickupMeUp.name);
    }

    // Update is called once per frame
    void Update()
    {
        if (activated)
        {
            transform.Rotate(0, speed, 0);
            
            RaycastHit hit;
            Physics.Raycast(transform.position, transform.forward, out hit, 99999f);
            Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.red);
            
            GetComponent<LineRenderer>().SetPosition(1, hit.point);

            if (CompareTag("Alien")) // tag == "Alien"
            {
                
            }
        }
    }
}