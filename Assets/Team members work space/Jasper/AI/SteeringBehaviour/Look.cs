using System;
using System.Collections.Generic;
using UnityEngine;

public class Look : MonoBehaviour
{
    [SerializeField] private float fieldOfView;
    [SerializeField] private float sightDistance;
    [SerializeField] private float reachDistance; 
    [SerializeField] private int maxRays;
    [SerializeField] private LayerMask floor;

    private float _rayAngle;
    private int _rayCount;
    private Vector3 _start;
    
    public float Reach => reachDistance;
    
    public List<RaycastHit> LookAround()
    {
        List<RaycastHit> inView = new List<RaycastHit>();
        
        _rayCount = maxRays;
        _rayAngle = 0;
        _start = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward;

        for (int i = 0; i < _rayCount; i++)
        {
            if (Physics.Raycast(transform.position, Quaternion.Euler(0, _rayAngle, 0) * _start, 
                    out RaycastHit hit, sightDistance))
            {
                inView.Add(hit); 
                //Debug.DrawLine(transform.position, hit.point, Color.cyan);
            }
            
            _rayAngle += fieldOfView / _rayCount; 
        }

        return inView;
    }

    /// <summary>
    /// Return seen objects within the passed layer mask
    /// </summary>
    /// <param name="observableMask"></param>
    /// <returns></returns>
    public List<RaycastHit> LookAround(LayerMask observableMask)
    {
        List<RaycastHit> inView = new List<RaycastHit>();
        
        _rayCount = maxRays;
        _rayAngle = 0;
        _start = Quaternion.Euler(0, -fieldOfView / 2, 0) * transform.forward;

        for (int i = 0; i < _rayCount; i++)
        {
            //Debug.DrawRay(transform.position, Quaternion.Euler(0, _rayAngle, 0) * _start, Color.cyan);
            
            if (Physics.Raycast(transform.position + (transform.up * .1f), Quaternion.Euler(0, _rayAngle, 0) * _start, 
                    out RaycastHit hit, sightDistance, observableMask))
            {
                inView.Add(hit); 
            }
            
            _rayAngle += fieldOfView / _rayCount; 
        }

        return inView;
    }

    /// <summary>
    /// Check the reachable surroundings of the character
    /// </summary>
    /// <returns>Colliders in the reachable range</returns>
    public Collider[] CheckReachableDistance()
    {
        return Physics.OverlapSphere(transform.position, reachDistance);
    }

    public Collider[] CheckSurroundingArea(Vector3 center, float radius)
    {
        return Physics.OverlapSphere(center, radius);
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.green;
    //     Gizmos.DrawWireSphere(transform.position, reachDistance);
    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawWireSphere(transform.position, sightDistance);
    // }
}
