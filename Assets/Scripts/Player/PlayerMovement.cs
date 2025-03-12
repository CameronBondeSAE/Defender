using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float moveSpeed = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector2 inputVector = PlayerInputHandler.instance.moveInput;
        Vector3 moveDirection = new Vector3(inputVector.x, 0, inputVector.y);
        
        // Quaternion moveRotation = Quaternion.LookRotation(moveDirection);
        // rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
        Quaternion cameraRotation = Quaternion.Euler(0, 45, 0);
        moveDirection = cameraRotation * moveDirection;
        rb.linearVelocity = moveDirection * moveSpeed;
    }
}
