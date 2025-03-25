using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler instance { get; private set; }
    [Header("Input Actions")]
    public InputAction moveAction;
    public InputAction shootAction;
    public InputAction throwAction; 
    public Vector2 moveInput { get; private set; }
    
    PlayerCombat playerCombat;

    void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
        playerCombat = GetComponent<PlayerCombat>();
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
            shootAction.performed += OnShootPerformed;
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
            shootAction.performed -= OnShootPerformed;
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
    
    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        playerCombat.FireBullet();
    }
    
    private void OnThrowPerformed(InputAction.CallbackContext context)
    {
        playerCombat.ThrowGrenade();
    }
}
