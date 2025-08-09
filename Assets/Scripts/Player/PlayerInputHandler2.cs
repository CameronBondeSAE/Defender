using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Unity.Netcode;

public class PlayerInputHandler2 : NetworkBehaviour
{
	public Vector2 moveInput { get; private set; }

	public PlayerInput        input;
	public event Action<bool> onUse;
	public event Action onInventory;

	override public void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		if (!IsLocalPlayer)
		{
			Debug.Log(gameObject.name + " : Not local player");
			return;
		}
		
		input = GetComponent<PlayerInput>();

		InputAction move      = input.actions.FindAction("Player/Move");
		InputAction throwing  = input.actions.FindAction("Player/Throw");
		InputAction use       = input.actions.FindAction("Player/Use");
		InputAction inventory = input.actions.FindAction("Player/Inventory");

		move.performed += OnMovePerformed;
		move.canceled  += OnMovePerformed;
		// throwing.performed  += OnThrowPerformed;
		use.performed       += OnUsePerformed;
		inventory.performed += OnInventoryPerformed;
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();

		if (!IsLocalPlayer)
		{
			return;
		}
		
		InputAction move      = input.actions.FindAction("Player/Move");
		InputAction throwing  = input.actions.FindAction("Player/Throw");
		InputAction use       = input.actions.FindAction("Player/Use");
		InputAction inventory = input.actions.FindAction("Player/Inventory");

		move.performed -= OnMovePerformed;
		move.canceled  -= OnMovePerformed;
		// throwing.performed  -= OnThrowPerformed;
		use.performed       -= OnUsePerformed;
		inventory.performed -= OnInventoryPerformed;
	}

	private void OnMovePerformed(InputAction.CallbackContext context)
	{
		moveInput = context.ReadValue<Vector2>();
	}

	private void OnUsePerformed(InputAction.CallbackContext obj)
	{
		if (obj.performed)
		{
			onUse?.Invoke(true);
		}
		else if (obj.canceled)
		{
			onUse?.Invoke(false);
		}
	}

	private void OnInventoryPerformed(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			onInventory?.Invoke();
		}
	}
}