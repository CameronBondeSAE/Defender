using UnityEngine;
using System.Collections.Generic;

public class VisionSystem : MonoBehaviour
{
    [Header("Vision Settings")]
    public float visionRange = 15f;
    public float visionAngle = 120f;
    public LayerMask visionMask;
    public float memoryDuration = 0.25f;

    private Dictionary<GameObject, float> visibleObjects = new Dictionary<GameObject, float>();

    private void FixedUpdate()
    {
        Scan();
        CleanupMemory();
    }

    private void Scan()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange, visionMask);

        foreach (var hit in hits)
        {
            GameObject obj = hit.gameObject;

            if (!IsInFOV(obj.transform.position))
            {
                continue;
            }

            if (!HasLineOfSight(obj))
            {
                continue;
            }
            
            visibleObjects[obj] = Time.time;
            // Debug.DrawRay(transform.position, (obj.transform.position - transform.position).normalized * visionRange, Color.yellow, 0.02f);
        }
    }

    private void CleanupMemory()
    {
        List<GameObject> toRemove = new List<GameObject>();

        foreach (var pair in visibleObjects)
        {
            GameObject obj = pair.Key;

            if (Time.time - pair.Value > memoryDuration)
            {
                toRemove.Add(obj);
            }
        }

        foreach (var obj in toRemove)
        {
            visibleObjects.Remove(obj);
        }
    }


    private bool IsInFOV(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        return Vector3.Angle(transform.forward, dir) <= (visionAngle * 0.5f);
    }

    private bool HasLineOfSight(GameObject target)
    {
        Vector3 dir = (target.transform.position - transform.position).normalized;

        return Physics.Raycast(transform.position, dir, out RaycastHit hit, visionRange, visionMask) && hit.collider.gameObject == target;
    }

    public bool CanSeeObject(GameObject obj)
    {
        if (obj == null)
        {
            return false;
        }
        return visibleObjects.ContainsKey(obj);
    }
    
    public bool GetClosestVisibleObjectWithTag(string tag, out GameObject closest)
    {
        closest = null;
        float bestDistance = Mathf.Infinity;

        foreach (var keyValuePair in visibleObjects)
        {
            GameObject obj = keyValuePair.Key;

            if (obj == null || !obj.CompareTag(tag))
            {
                continue;
            }

            float distance = (obj.transform.position - transform.position).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                closest = obj;
            }
        }

        return closest != null;
    }

    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }

}
