using Defender;
using UnityEngine;

public class RadarItem : UsableItem_Base
{
    [Header("Radar Item")] 
    [SerializeField] private float spinningSpeed = 1f;
    
    
    
    void Update()
    {
        transform.Rotate(0, spinningSpeed, 0);
        
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit, 10f);
        Debug.DrawRay(transform.position, transform.forward * hit.distance, Color.blue);
    }
}
