using Defender;
using UnityEngine;

public class DecoyItem : AIBase, IUsable, IPickup
{
	// when is picked up it tells the player it is a decoy item

	protected override void Start()
	{
	}

	void Update()
	{
		if (IsAbducted == true)
		{
			Destroy();
		}
	}

	public void Destroy()
	{
		GameObject.Destroy(gameObject);
	}

	public void Use(CharacterBase characterTryingToUse)
	{
		Debug.Log("Hello fellow citizens :)");
	}

	public void StopUsing()
	{
	}

	public void Pickup()
	{
		// StopUsing();
	}

	public void Drop()
	{
	}
}