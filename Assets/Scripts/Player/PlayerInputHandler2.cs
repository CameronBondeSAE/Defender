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

		move.performed += OnMoveUpdated;
		move.canceled  += OnMoveUpdated;
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

		move.performed -= OnMoveUpdated;
		move.canceled  -= OnMoveUpdated;
		// throwing.performed  -= OnThrowPerformed;
		use.performed       -= OnUsePerformed;
		inventory.performed -= OnInventoryPerformed;
	}

	private void OnMoveUpdated(InputAction.CallbackContext context)
	{
		RequestMovePerformed_Rpc(context.ReadValue<Vector2>());
	}

	[Rpc(SendTo.Server, RequireOwnership = true, Delivery = RpcDelivery.Unreliable)]
	private void RequestMovePerformed_Rpc(Vector2 _moveInput)
	{
		moveInput = _moveInput;
	}

	
	
	private void OnUsePerformed(InputAction.CallbackContext obj)
	{
		if (obj.performed)
			RequestTryUseItem_Rpc(true);
		else
			RequestTryUseItem_Rpc(false);
	}

	[Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
	private void RequestTryUseItem_Rpc(bool state)
	{
		if (state)
		{
			onUse?.Invoke(true);
		}
		else
		{
			onUse?.Invoke(false);
		}
	}

	private void OnInventoryPerformed(InputAction.CallbackContext context)
	{
		RequestTryPickupItem_Rpc();
	}
	
	[Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
	private void RequestTryPickupItem_Rpc()
	{
		onInventory?.Invoke();
	}


}