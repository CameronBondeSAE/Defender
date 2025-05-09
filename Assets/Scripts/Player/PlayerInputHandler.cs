using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Actions")]
    public InputAction moveAction;
    public InputAction shootAction;
    public InputAction throwAction;
    public InputAction laserAction;
    public InputAction aimGrenadeAction;
    public InputAction interactAction;
    public Vector2 moveInput { get; private set; }
    
    public event Action onShootStart;
    public event Action onShootStop;
    public event Action onThrow;
    
    public event Action onInteract;

    public event Action onLaserStart;
    public event Action onLaserStop;
    public event Action onAimGrenadeStart;
    public event Action onAimGrenadeStop;

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
            laserAction.started += OnLaserActivated;
            laserAction.canceled += OnLaserDeactivated;
        }
        
        // Throw (grenade)
        if (throwAction != null)
        {
            throwAction.Enable();
            throwAction.performed += OnThrowPerformed;
        }
       
        // Aim Grenade
        if (aimGrenadeAction != null)
        {
            aimGrenadeAction.Enable();
            aimGrenadeAction.started += OnAimGrenadeStarted;
            aimGrenadeAction.canceled += OnAimGrenadeStopped;
        }
        
        if (interactAction != null)
        {
	        interactAction.Enable();
	        interactAction.performed += OnInteractPerformed;
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
            laserAction.started -= OnLaserActivated;
            laserAction.canceled -= OnLaserDeactivated;
        }
        
        if (throwAction != null)
        {
            throwAction.Disable();
            throwAction.performed -= OnThrowPerformed;
        }
        
        if (aimGrenadeAction != null)
        {
            aimGrenadeAction.Disable();
            aimGrenadeAction.started -= OnAimGrenadeStarted;
            aimGrenadeAction.canceled -= OnAimGrenadeStopped;
        }
        
        if (interactAction != null)
        {
	        interactAction.Disable();
	        interactAction.performed -= OnInteractPerformed;
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
    
    private void OnLaserActivated(InputAction.CallbackContext context)
    {
        onLaserStart?.Invoke();
    }
    
    private void OnLaserDeactivated(InputAction.CallbackContext context)
    {
        onLaserStop?.Invoke();
    }
    
    private void OnThrowPerformed(InputAction.CallbackContext context)
    {
        onThrow?.Invoke();
    }
    
    private void OnAimGrenadeStarted(InputAction.CallbackContext context)
    {
        onAimGrenadeStart?.Invoke();
    }

    private void OnAimGrenadeStopped(InputAction.CallbackContext context)
    {
        onAimGrenadeStop?.Invoke();
    }
    
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
	    onInteract?.Invoke();
    }
}
