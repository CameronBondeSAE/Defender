using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler instance { get; private set; }
    [Header("Input Actions")]
    public InputAction moveAction;
    public InputAction shootAction;
    public InputAction throwAction; 
    public Vector2 moveInput { get; private set; }
    
    public event Action onShootStart;
    public event Action onShootStop;
    public event Action onThrow;

    private bool isShooting = false;
    

    void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Move
        if (moveAction != null)
        {
            moveAction.Enable();
            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;
        }
        
        // Shoot
        if (shootAction != null)
        {
            shootAction.Enable();
            shootAction.performed += OnShootStarted;
            shootAction.performed += OnShootStopped;
        }
        
        // Throw (grenade)
        if (throwAction != null)
        {
            throwAction.Enable();
            throwAction.performed += OnThrowPerformed;
        }
    }
    
    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.Enable();
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
        }
        
        if (shootAction != null)
        {
            shootAction.Disable();
            shootAction.performed -= OnShootStarted;
            shootAction.canceled -= OnShootStopped;
        }
        
        if (throwAction != null)
        {
            throwAction.Disable();
            throwAction.performed -= OnThrowPerformed;
        }
    }
    
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }
    
    private void OnShootStarted(InputAction.CallbackContext context)
    {
        onShootStart?.Invoke();
    }
    
    private void OnShootStopped(InputAction.CallbackContext context)
    {
        onShootStop?.Invoke();
    }
    
    private void OnThrowPerformed(InputAction.CallbackContext context)
    {
        onThrow?.Invoke();
    }
}
