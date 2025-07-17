using UnityEngine;

public class Teleporter : MonoBehaviour, IInteractable
{
	public int enegyRequired;
	public int damageDone;
	
	public void Interact()
	{
		Teleport();
	}

	public void Teleport()
	{
		Debug.Log("Teleport");
	}
}
