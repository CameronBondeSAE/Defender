using UnityEngine;

public class AirConUnit : MonoBehaviour
{
    [SerializeField] private float pushForce;

    [SerializeField] private float rotationSpeed;
    [SerializeField] private float angleRange; 

    private void FixedUpdate()
    {
        Swing();
    }

    private void OnTriggerStay(Collider other)
    {
         Rigidbody rb = other.GetComponent<Rigidbody>();
         if (rb != null)
         {
            Vector3 pushDirection = transform.forward;
            rb.AddForce(pushDirection * pushForce, ForceMode.Force);
         }
    }

    /// <summary>
    /// makes aircon rotate side to side
    /// </summary>
    private void Swing()
    {
        float angleOffset = Mathf.Sin(Time.time * rotationSpeed) * (angleRange / 2f); //range is halfed so it rotates x degrees to each side that adds up to the max range
        transform.rotation = Quaternion.Euler(0, angleOffset, 0);
    }

}