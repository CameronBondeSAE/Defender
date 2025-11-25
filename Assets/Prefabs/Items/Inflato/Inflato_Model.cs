using Defender;
using DG.Tweening;
using UnityEngine;

public class Inflato_Model : UsableItem_Base
{
	public Renderer       renderer;
	public Rigidbody      rb;
	public ParticleSystem ps;
	
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
		ps.Emit(20);
		transform.DOScale(new Vector3(1f,1f,1f), 1f).OnComplete(CompletedEffects);
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
		renderer.material.color = colour;
	}
}
