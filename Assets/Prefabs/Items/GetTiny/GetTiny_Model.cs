using Defender;
using DG.Tweening;
using UnityEngine;

public class GetTiny_Model : UsableItem_Base
{
	public override void Pickup(CharacterBase whoIsPickupMeUp)
	{
		base.Pickup(whoIsPickupMeUp);
		
		Debug.Log("GetTiny_Model picked up by : "+whoIsPickupMeUp.name);
	}

	public override void Use(CharacterBase characterTryingToUse)
	{
		base.Use(characterTryingToUse);
		
		Debug.Log("Use GetTiny_Model : By "+characterTryingToUse.name);
		
		transform.DOScale(0.5f, 1f).SetEase(Ease.InOutElastic);
	}

	public override void StopUsing()
	{
		base.StopUsing();

		Debug.Log("GetTiny_Model picked up : StopUsing");
		
		transform.DOScale(1f, 1f).SetEase(Ease.InOutElastic);
	}
}
