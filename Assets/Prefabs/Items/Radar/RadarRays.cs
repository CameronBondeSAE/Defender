using System;
using Defender;
using UnityEngine;

public class RadarRays : MonoBehaviour
{
	public float speed = 10f;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
	    transform.Rotate(0,speed,0);

	    RaycastHit hit;
	    Physics.Raycast(transform.position, transform.forward, out hit, 99999f);
		Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.red);
	    
	    if (hit.transform != null)
	    {
		    // Physics.Raycast(hit.point, hit.normal, out hit, 99999f);
		    var direction = Vector3.Reflect(transform.forward, hit.normal);
		    Physics.Raycast(hit.point, direction, out RaycastHit hit2, 99999f);
		    Debug.DrawRay(hit.point, direction * hit2.distance, Color.green);
		    
	    }
	    
    }
}
