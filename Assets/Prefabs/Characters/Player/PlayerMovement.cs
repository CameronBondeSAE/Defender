using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody rb;
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

    // WHY isn't speed variable in here? Because we put it in characterBase so we could alter the speed of ANY character with items, without hardcoding to PlayerMovement etc. We could also have used this script on the NPCs, but not all NPCs move the same
    public Player_Model playerModel;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        playerCombat = GetComponent<PlayerCombat>();
        inputHandler = GetComponent<PlayerInputHandler2>();
        playerAnimation = GetComponent<PlayerAnimation>();
        playerModel = GetComponent<Player_Model>();
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

        Vector3 targetVelocity = moveDirection * playerModel.MoveSpeed;
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float velocityMagnitude = horizontalVelocity.magnitude;

        if (velocityMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(horizontalVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * playerModel.MoveSpeed);
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
