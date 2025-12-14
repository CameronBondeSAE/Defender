using System;
using System.Collections.Generic;
using UnityEngine;

public class Look : MonoBehaviour
{
    [SerializeField] private float fieldOfView;
    [SerializeField] private float sightDistance;
    [SerializeField] private float reachDistance; 
    [SerializeField] private int maxRays;
    [SerializeField] private float eyeLevel;
    [SerializeField] private LayerMask floor;

    private float _rayAngle;
    private int _rayCount;
    private Vector3 _start;
    private Vector3 _eyeLevelOffset;

    private void OnEnable()
    {
        _eyeLevelOffset = Vector3.up * eyeLevel;
    }
    
    public List<RaycastHit> LookAround()
    {
        List<RaycastHit> inView = new List<RaycastHit>();
        
        _rayCount = maxRays; //TODO: change the number of rays based on distance from camera (maybe) 
        _rayAngle = 0;
        _start = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward;

        for (int i = 0; i < _rayCount; i++)
        {
            Debug.DrawRay(transform.position, Quaternion.Euler(0, _rayAngle, 0) * _start, Color.cyan);
            
            if (Physics.Raycast(transform.position, Quaternion.Euler(0, _rayAngle, 0) * _start, 
                    out RaycastHit hit, sightDistance))
            {
                inView.Add(hit); 
            }
            
            _rayAngle += fieldOfView / _rayCount; 
        }

        return inView;
    }

    public List<RaycastHit> LookAround(LayerMask observableMask)
    {
        List<RaycastHit> inView = new List<RaycastHit>();
        
        _rayCount = maxRays; //TODO: change the number of rays based on distance from camera (maybe) 
        _rayAngle = 0;
        _start = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward;

        for (int i = 0; i < _rayCount; i++)
        {
            Debug.DrawRay(transform.position, Quaternion.Euler(0, _rayAngle, 0) * _start, Color.cyan);
            
            if (Physics.Raycast(transform.position, Quaternion.Euler(0, _rayAngle, 0) * _start, 
                    out RaycastHit hit, sightDistance, observableMask))
            {
                inView.Add(hit); 
            }
            
            _rayAngle += fieldOfView / _rayCount; 
        }

        return inView;
    }

    public List<RaycastHit> EdgeCheck()
    {
        List<RaycastHit> edgeRays = new List<RaycastHit>();
        
        RaycastHit edgeHit;

        _start = transform.position + _eyeLevelOffset;
        
        Debug.DrawRay(_start + (transform.forward * 2), Vector3.down, Color.cyan);
        Physics.Raycast(_start + (transform.forward * 2), Vector3.down, out edgeHit); //in front
        edgeRays.Add(edgeHit);

        _start += transform.forward * .5f;
        
        Debug.DrawRay(_start + transform.right * 1.2f, Vector3.down, Color.cyan);
        Physics.Raycast(_start + transform.right * 1.2f, Vector3.down, out edgeHit); //left
        edgeRays.Add(edgeHit);
        
        Debug.DrawRay(_start - transform.right * 1.2f, Vector3.down, Color.cyan);
        Physics.Raycast(_start - transform.right * 1.2f, Vector3.down, out edgeHit); //right
        edgeRays.Add(edgeHit);

        return edgeRays;
    }

    /// <summary>
    /// Check the reachable surroundings of the character
    /// </summary>
    /// <returns>Colliders in the reachable range</returns>
    public Collider[] CheckReachableDistance()
    {
        return Physics.OverlapSphere(transform.position, reachDistance);
    }
}
