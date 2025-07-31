using UnityEngine;

public class ExplodyMine : MonoBehaviour, IUsable
{
	public void Explode()
    {
	    Debug.Log("Explode");
    }

	public void Use()
	{
		Explode();
	}

	public void StopUsing()
	{
		throw new System.NotImplementedException();
	}
}
