using UnityEngine;
using Inventory.SO;
using UnityEngine.InputSystem;

public class BasicGrenade : MonoBehaviour
{
    public ItemSO grenadeData;
    private float countdown;
    private float launchSpeed;
    private Rigidbody rb;
    private Transform startPosition;

    private void Start()
    {
        countdown = grenadeData.explosionDelay;
        launchSpeed = grenadeData.launchSpeed;
        //startPosition.position = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        countdown -= Time.deltaTime;
        if (countdown <= 0f) Explode();
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Launch();
        }
    }

    public void Launch()
    {
        var launchDirection = transform.forward + transform.up * 1f;
        rb.AddForce(launchDirection * launchSpeed, ForceMode.VelocityChange);
    }

    private void Explode()
    {
        var colliders = Physics.OverlapSphere(transform.position, grenadeData.explosionRadius);
        foreach (var collider in colliders)
        {
            var health = collider.GetComponent<Health>();
            if (health != null) health.TakeDamage(grenadeData.damage);
        }

        Destroy(gameObject);
    }
}