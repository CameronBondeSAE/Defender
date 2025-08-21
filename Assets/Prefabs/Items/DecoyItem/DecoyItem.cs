using Defender;
using UnityEngine;


public class DecoyItem : UsableItem_Base // UsableItem_Base
{
	// when is picked up it tells the player it is a decoy item
	[SerializeField] private bool blnIsPlaced = true;
	[SerializeField] private bool blnIsActive = false;
	public AudioClip deployClip, deathClip;
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private GameObject[] decoyVeiw;
	public bool blnDie = false;
	
	protected override void Awake()
	{
		base.Awake();
		IsDecoy(false);


		
		
	}

	void Update()
	{
		if (blnDie)
		{
			Death_Rpc();
			blnDie = false;
		}
	}

	

	public override void Use(CharacterBase characterTryingToUse)
	{
		if (blnIsActive)
		{
			StopUsing();
		}
		else
		{
			base.Use(characterTryingToUse);
			Debug.Log("Hello fellow citizens :)");
			blnIsActive = true;
			if (blnIsPlaced)
			{
				//colour blue
				IsDecoy(true);
			
			}
			else
			{
				//colour green

				Use_Rpc();
			}
		}
		
	}

	public override void StopUsing()
	{
		
		blnIsActive = false;
		if (blnIsPlaced == false)
		{
			base.StopUsing();
			//colour grey
		
			StopUsing_rpc();
		}
	
	}

	public override void Pickup(CharacterBase whoIsPickupMeUp)
	{
		
			Debug.Log("Picked up item");
			blnIsPlaced = false;
			if (blnIsActive)
			{
				//cant pick back up
			}
			
	
		
		
	}

	public override void Drop()
	{
		base.Drop();
		blnIsPlaced = true;
		if (blnIsActive)
		{
			//colour blue 
			IsDecoy(true);
			
		}
	}

	public void IsDecoy(bool blnIsDecoy)
	{
		if (blnIsDecoy)
		{
			transform.tag = "Civilian";
			DecoyActiveate_Rpc();
		}
	
	}

	
	
	public void Use_Rpc()
	{
		for (int i = 0; i < decoyVeiw.Length  ; i++)
		{
			decoyVeiw[i].GetComponent<Renderer>().material.color = Color.green;
			Debug.Log(i);
		}
		
	}

	public void StopUsing_rpc()
	{
		for (int i = 0; i < decoyVeiw.Length; i++)
		{
			decoyVeiw[i].GetComponent<Renderer>().material.color = Color.gray;
		}
		
		
	}

	public void DecoyActiveate_Rpc()
	{
		if (audioSource && deployClip) audioSource.PlayOneShot(deployClip);
		for (int i = 0; i < decoyVeiw.Length; i++)
		{
			decoyVeiw[i].GetComponent<Renderer>().material.color = Color.deepSkyBlue;
		}
		
	}

	public void Death_Rpc()
	{
		if (audioSource && deathClip) audioSource.PlayOneShot(deathClip);
		audioSource.PlayOneShot(deathClip);
	}
}