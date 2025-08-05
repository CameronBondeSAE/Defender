using UnityEngine;

public class Snack : MonoBehaviour
{
    [Header("Snack Params")]
    public GameObject snackPrefab;
    public int civsToAttract = 3; 
    
    private GameObject heldSnack;
    private bool isCarrying = false;
    private Transform playerTransform;
    
    private void Start()
    {
        playerTransform = transform; // or assign directly if in inventory
    }

    public void Use()
    {
        if (!isCarrying && snackPrefab != null)
        {
            // spawn snack in player's hand or at player position
            heldSnack = Instantiate(snackPrefab, playerTransform.position + Vector3.up * 1f, Quaternion.identity);
            heldSnack.transform.SetParent(playerTransform);
            heldSnack.transform.localPosition = new Vector3(0, 1f, 1f); // adjust for "holding"
            isCarrying = true;

            var snackScript = heldSnack.GetComponent<SnackObject>();
            if (snackScript != null)
            {
                snackScript.Setup(this, this);
            }

            Debug.Log("[SnackBomb] Snack picked up");
        }
    }

    public void StopUsing()
    {
        if (isCarrying && heldSnack != null)
        {
            // drop snack
            heldSnack.transform.SetParent(null);
            heldSnack.transform.position = playerTransform.position + playerTransform.forward * 1.5f + Vector3.up * 0.2f;

            var snackScript = heldSnack.GetComponent<SnackObject>();
            if (snackScript != null)
            {
                snackScript.OnSnackDropped(civsToAttract);
            }
            isCarrying = false;
            heldSnack = null;

            Debug.Log("[SnackBomb] Snack dropped");
        }
    }
}
