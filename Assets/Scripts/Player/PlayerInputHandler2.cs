using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputHandler2 : MonoBehaviour
{
	public Vector2 moveInput { get; private set; }
    
    public event Action onShootStart;
    public event Action onShootStop;
    public event Action onThrow;
    
    public event Action onInteract;

    public event Action onLaserStart;
    public event Action onLaserStop;
    public event Action onAimGrenadeStart;
    public event Action onAimGrenadeStop;

    public bool isShooting = false;
    
    public PlayerInput input;
    
    private void OnEnable()
    {
	    input = GetComponent<PlayerInput>();
	    
	    
	    InputAction move = input.actions.FindAction("Player/Move");
	    InputAction throwing = input.actions.FindAction("Player/Throw");
	    InputAction interact = input.actions.FindAction("Player/Interact");
	    
	    move.performed += OnMovePerformed;
	    move.canceled += OnMovePerformed;
		throwing.performed += OnThrowPerformed;
	    interact.performed += OnInteractPerformed;
    }
    
    private void OnDisable()
    {
	    InputAction move = input.actions.FindAction("Player/Move");
	    InputAction throwing = input.actions.FindAction("Player/Throw");
	    InputAction interact = input.actions.FindAction("Player/Interact");
	    
	    move.performed -= OnMovePerformed;
	    move.canceled -= OnMovePerformed;
	    throwing.performed -= OnThrowPerformed;
	    interact.performed -= OnInteractPerformed;
    }
    
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
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
