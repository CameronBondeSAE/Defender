using UnityEngine;

public class AirConUnit : MonoBehaviour
{
    [SerializeField] private float pushForce = 10f;

    private void OnTriggerStay(Collider other)
    {
         Rigidbody rb = other.GetComponent<Rigidbody>();
         if (rb != null)
         {
            Vector3 pushDirection = transform.forward;
            rb.AddForce(pushDirection * pushForce, ForceMode.Force);
         }
    }

}