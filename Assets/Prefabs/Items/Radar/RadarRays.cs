using System;
using Defender;
using UnityEngine;

public class RadarRays : UsableItem_Base
{
	public float speed     = 10f;
	public bool  activated = false;
	public RadarView radarView;

	public override void Use(CharacterBase characterTryingToUse)
	{
		base.Use(characterTryingToUse);

		activated = !activated;
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

			if (hit.transform != null)
			{
				radarView.RadarSound_RPC();
				// Physics.Raycast(hit.point, hit.normal, out hit, 99999f);
				var direction = Vector3.Reflect(transform.forward, hit.normal);
				Physics.Raycast(hit.point, direction, out RaycastHit hit2, 99999f);
				Debug.DrawRay(hit.point, direction * hit2.distance, Color.green);
			}
		}
	}
}