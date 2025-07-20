using UnityEngine;

[RequireComponent(typeof(Collider), typeof(FloatingUI))]
public class PickupItem : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField]
    private float interactionRange = 2f;
    
    private FloatingUI floatingUI;
    private bool playerInRange = false;
    private Transform playerTransform;

    private void Start()
    {
        floatingUI = GetComponent<FloatingUI>();
        // Ensure the trigger collider is set up in case forget
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerTransform = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerTransform = null;
        }
    }

    /// <summary>
    /// Check if player is in range for pickup
    /// </summary>
    public bool IsPlayerInRange(Transform player)
    {
        if (!playerInRange || playerTransform == null) 
            return false;
            
        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= interactionRange;
    }

    /// <summary>
    /// Called when the item is successfully picked up
    /// </summary>
    public void OnPickedUp()
    {
        if (floatingUI != null)
        {
            floatingUI.OnPickedUp();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnDrawGizmosSelected()
    {
        // Visual debug
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}