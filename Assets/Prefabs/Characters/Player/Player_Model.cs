using System;
using Defender;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Model : CharacterBase
{
	public PlayerHealth health;
	public PlayerInput playerInput;

	private void OnEnable()
	{
		if (health != null)
		{
			health.OnDeath  += HealthOnOnDeath;
			health.OnRevive += HealthOnOnRevive;
		}
	}

	private void OnDisable()
	{
		if (health != null)
		{
			health.OnDeath  -= HealthOnOnDeath;
			health.OnRevive -= HealthOnOnRevive;
		}
	}

	private void HealthOnOnDeath()
	{
		playerInput.DeactivateInput();
	}

	private void HealthOnOnRevive()
	{
		playerInput.ActivateInput();
	}
}
