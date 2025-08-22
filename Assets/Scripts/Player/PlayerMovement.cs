using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private Camera gameCamera;
    
    private bool isMovingForAnim = false;
    private const float RunStartSpeed = 0.20f; // start running when above this
    private const float RunStopSpeed  = 0.05f; 
    
    public Vector3 moveDirection;
    private PlayerCombat playerCombat;
    private PlayerInputHandler2 inputHandler;
    private PlayerAnimation playerAnimation;

    Vector2 inputVector;

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        playerCombat = GetComponent<PlayerCombat>();
        inputHandler = GetComponent<PlayerInputHandler2>();
        playerAnimation = GetComponent<PlayerAnimation>();
    }

    private void OnEnable()
    {
        inputHandler.onMove += OnMovefff;
    }
    
    private void OnDisable()
    {
        inputHandler.onMove -= OnMovefff;
    }

    private void OnMovefff(Vector2 obj)
    {
        inputVector = obj;
    }

    void FixedUpdate()
    {
        // Vector2 inputVector = inputHandler.moveInput;
        Vector3 moveDirection = new Vector3(inputVector.x, 0, inputVector.y);

        Vector3 targetVelocity = moveDirection * moveSpeed;
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float velocityMagnitude = horizontalVelocity.magnitude;

        if (velocityMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * moveSpeed);
            // playerAnimation.RequestState(PlayerAnimation.PlayerState.Run);
        }
        else
        {
            // playerAnimation.RequestState(PlayerAnimation.PlayerState.Idle);
        }
        
        if (isMovingForAnim)
        {
            if (velocityMagnitude < RunStopSpeed) isMovingForAnim = false;
        }
        else
        {
            if (velocityMagnitude > RunStartSpeed) isMovingForAnim = true;
        }
        if (IsServer)
        {
            playerAnimation.RequestState(
                isMovingForAnim ? PlayerAnimation.PlayerState.Run
                    : PlayerAnimation.PlayerState.Idle);
        }
    }
}
