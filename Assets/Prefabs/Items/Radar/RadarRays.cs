using System;
using Defender;
using UnityEngine;

public class RadarRays : UsableItem_Base
{
	public float speed     = 10f;
	public bool activated;

	public override void Use(CharacterBase characterTryingToUse)
	{
		base.Use(characterTryingToUse);

		activated = !activated;
	}
	public override void Pickup(CharacterBase whoIsPickupMeUp)
	{
		base.Pickup(whoIsPickupMeUp);
		
		Debug.Log(whoIsPickupMeUp.name);
	}
	void Update()
	{
		if (activated)
		{
			transform.Rotate(0, speed, 0);
			
			RaycastHit hit;
			Physics.Raycast(transform.position, transform.forward, out hit, 99999f);
			Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.red);
		}
	}
}