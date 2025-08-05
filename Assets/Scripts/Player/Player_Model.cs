using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Model : MonoBehaviour
{

	public PlayerHealth health;
	public PlayerInput playerInput;

	private void OnEnable()
	{
		health.OnDeath += HealthOnOnDeath;
		health.OnRevive += HealthOnOnRevive;
	}

	private void OnDisable()
	{
		health.OnDeath -= HealthOnOnDeath;
		health.OnRevive -= HealthOnOnRevive;
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
