using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Unity.Netcode;

public class PlayerInputHandler2 : NetworkBehaviour
{
	// public Vector2 moveInput { get; private set; }

	public PlayerInput        input;
	public event Action<Vector2> onMove;
	public event Action<bool> onUse;
	public event Action onInventory;
	
	public bool InputEnabled { get; private set; } = true;

	private void Awake()
	{
		input ??= GetComponent<PlayerInput>();
		
		if (input?.actions == null)
		{
			Debug.LogError($"[{gameObject.name}] PlayerInput or actions asset missing!");
			return;
		}
		else
		{
			// Debug.Log($"[{gameObject.name}] PlayerInput found!");
		}
		DontDestroyOnLoad(this);
	}

	override public void OnNetworkSpawn()
	{
		// Debug.Log($"[{gameObject.name}] OnNetworkSpawn called - IsLocalPlayer: {IsLocalPlayer}");

		base.OnNetworkSpawn();

		// IMPORTANT: Input component DOESN'T WORK with netcode I think???
		if (!IsLocalPlayer)
		{
			Debug.Log(gameObject.name + " : Not local player");
			// Disable the entire PlayerInput component for non-local players
			if (input != null) 
				input.enabled = false;
			return;
		}
		else
		{
			// Debug.Log(gameObject.name + " : Local player");
			input.enabled = true;
		}
		
		// Force input to use this specific device (helps with multiple instances)
		input.SwitchCurrentActionMap("Player");
		
		// Debug.Log("PlayerInputHandler2 : OnNetworkSpawn");
		
		InputAction moveAction      = input.actions.FindAction("Player/Move");
		InputAction useAction       = input.actions.FindAction("Player/Use");
		InputAction inventoryAction = input.actions.FindAction("Player/Inventory");
		
		input.actions.FindActionMap("Player").Enable();
		
		// Validate all actions exist
		if (moveAction == null) Debug.LogError("Move action not found!");
		if (useAction == null) Debug.LogError("Use action not found!");
		if (inventoryAction == null) Debug.LogError("Inventory action not found!");
		
		moveAction.performed += OnMoveUpdated;
		moveAction.canceled  += OnMoveUpdated;
		useAction.performed       += OnUsePerformed;
		inventoryAction.performed += OnInventoryPerformed;
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();

		if (!IsLocalPlayer)
		{
			return;
		}
		
		InputAction move      = input.actions.FindAction("Player/Move");
		InputAction use       = input.actions.FindAction("Player/Use");
		InputAction inventory = input.actions.FindAction("Player/Inventory");

		move.performed -= OnMoveUpdated;
		move.canceled  -= OnMoveUpdated;
		use.performed       -= OnUsePerformed;
		inventory.performed -= OnInventoryPerformed;
	}

	private void OnMoveUpdated(InputAction.CallbackContext context)
	{
		if (!InputEnabled)
			return;
		// Debug.Log("PlayerInputHandler2 : Move Updated : "+" "+context.ReadValue<Vector2>());
		RequestMovePerformed_Rpc(context.ReadValue<Vector2>());
	}

	[Rpc(SendTo.Server, RequireOwnership = true, Delivery = RpcDelivery.Unreliable)]
	private void RequestMovePerformed_Rpc(Vector2 _moveInput)
	{
		// Debug.Log("PlayerInputHandler SERVER : Move Requested : "+" "+_moveInput);
	
		onMove?.Invoke(_moveInput);
		// moveInput = _moveInput;
	}

	public void SetInputEnabled(bool enabled)
	{
		InputEnabled = enabled;
	}
	
	private void OnUsePerformed(InputAction.CallbackContext obj)
	{
		if (!InputEnabled)
			return;
		Debug.Log("OnUsePerformed : "+obj.performed);
		if (obj.performed)
			RequestTryUseItem_Rpc(true);
		else
			RequestTryUseItem_Rpc(false);
	}

	[Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
	private void RequestTryUseItem_Rpc(bool state)
	{
		Debug.Log("PlayerInputHandler SERVER : Use Requested : "+" "+state);
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
		if (!InputEnabled)
			return;
		Debug.Log("OnInventoryPerformed");
		RequestTryPickupItem_Rpc();
	}
	
	[Rpc(SendTo.Server, Delivery = RpcDelivery.Reliable, RequireOwnership = true)]
	private void RequestTryPickupItem_Rpc()
	{
		Debug.Log("PlayerInputHandler SERVER : Inventory Requested");
		onInventory?.Invoke();
	}

}