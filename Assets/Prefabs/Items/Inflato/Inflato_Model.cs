using System;
using Defender;
using DG.Tweening;
using NUnit.Framework;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Inflato_Model : UsableItem_Base
{
	public Renderer       renderer;
	public Rigidbody      rb;
	public ParticleSystem ps;
	
	private NetworkVariable<Color> networkColour = new NetworkVariable<Color>(Color.white, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

	void Awake()
	{
		networkColour.OnValueChanged += OnColourChanged;
	}

	private void OnColourChanged(Color previous, Color current)
	{
		renderer.material.color = current;
	}

	public override void Pickup(CharacterBase whoIsPickupMeUp)
	{
		base.Pickup(whoIsPickupMeUp);
		
		Debug.Log("GetTiny_Model picked up by : "+whoIsPickupMeUp.name);
	}

	public override void Use(CharacterBase characterTryingToUse)
	{
		base.Use(characterTryingToUse);
		
		Debug.Log("Use GetTiny_Model : By "+characterTryingToUse.name);
		
		transform.DOScale(new Vector3(3f,3f,3f), 1f).OnComplete(CompletedEffects).SetEase(Ease.InCubic);
		
		rb.mass = 100f;
		ChangeColour(Color.red);
	}

	void CompletedEffects()
	{
		ChangeColour(Color.green);
		ScaleEffects();
		transform.DOScale(new Vector3(1f,1f,1f), 1f).OnComplete(CompletedEffects);
	}

	private void ScaleEffects()
	{
		ps.Emit(20);
	}

	public override void StopUsing()
	{
		base.StopUsing();

		Debug.Log("GetTiny_Model picked up : StopUsing");
		
		transform.DOScale(1f, 1f).SetEase(Ease.InOutElastic);
		rb.mass = 1f;
		ChangeColour(Color.white);
	}

	public void ChangeColour(Color colour)
	{
		if (IsServer)
		{
			networkColour.Value = colour;
		}
	}
}
