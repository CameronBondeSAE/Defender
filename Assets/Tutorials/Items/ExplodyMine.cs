using UnityEngine;

public class ExplodyMine : MonoBehaviour, IInteractable
{
	public void Interact()
	{
		Explode();
	}

	public void Explode()
    {
	    Debug.Log("Explode");
    }
}
