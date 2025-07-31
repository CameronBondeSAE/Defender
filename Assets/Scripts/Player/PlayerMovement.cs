using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private Camera gameCamera;
    public Vector3 moveDirection;
    private PlayerCombat playerCombat;
    private PlayerInputHandler2 inputHandler;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        playerCombat = GetComponent<PlayerCombat>();
        inputHandler = GetComponent<PlayerInputHandler2>();
    }

    void FixedUpdate()
    {
        Vector2 inputVector = inputHandler.moveInput;
        moveDirection = new Vector3(inputVector.x, 0, inputVector.y);
        
        // rotate relative to camera
        // Quaternion cameraRotation = Quaternion.Euler(0, 0, 0);
        // moveDirection = cameraRotation * moveDirection;

        Vector3 targetVelocity = moveDirection * moveSpeed;
        // realistic movement
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        
        // player to face direction of his movement actually
        if (moveDirection.magnitude > 0.1 || moveDirection.magnitude > 0.1 && playerCombat.isShooting)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
            // PlayerEventManager.instance.events.onMove.Invoke();
            
            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
	            animator.Play("Run");
            }
        }
        else
        {
            // PlayerEventManager.instance.events.onIdle.Invoke();
            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
	            animator.Play("Idle");
            }
        }
    }
}
