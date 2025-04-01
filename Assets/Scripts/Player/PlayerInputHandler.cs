using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Actions")]
    public InputAction moveAction;
    public InputAction shootAction;
    public InputAction laserAction;
    public InputAction throwAction; 
    public Vector2 moveInput { get; private set; }
    
    public event Action onShootStart;
    public event Action onShootStop;
    
    public event Action onLaserStart;
    public event Action onLaserStop;
    public event Action onThrow;
    
    private bool isShooting = false;
    
    
    void Awake()
    {
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
            shootAction.started += OnShootStarted;
            shootAction.canceled += OnShootStopped;
        }
        
        // Laser
        if (laserAction != null)
        {
            laserAction.Enable();
            laserAction.started += OnShootStarted;
            laserAction.canceled += OnShootStopped;
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
            shootAction.started -= OnShootStarted;
            shootAction.canceled -= OnShootStopped;
        }
        
        if (laserAction != null)
        {
            laserAction.Enable();
            laserAction.started -= OnShootStarted;
            laserAction.canceled -= OnShootStopped;
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
        Debug.Log("shooting");
        onShootStart?.Invoke();
    }
    
    private void OnShootStopped(InputAction.CallbackContext context)
    {
        onShootStop?.Invoke();
    }
    
    private void OnLaserStarted(InputAction.CallbackContext context)
    {
        onLaserStart?.Invoke();
    }
    
    private void OnLaserStopped(InputAction.CallbackContext context)
    {
        onLaserStop?.Invoke();
    }
    
    private void OnThrowPerformed(InputAction.CallbackContext context)
    {
        onThrow?.Invoke();
    }
}
