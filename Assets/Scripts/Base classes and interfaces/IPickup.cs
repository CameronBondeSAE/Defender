using Defender;
using UnityEngine;

public interface IPickup
{
	void Pickup(CharacterBase whoIsPickupMeUp);
	void Drop();
}
