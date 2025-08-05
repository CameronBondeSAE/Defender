using UnityEngine;

public class DemoItem : MonoBehaviour, IUsable
{
    public void Use()
    {
	    Debug.Log("DemoItem Used");
    }

    public void StopUsing()
    {
	    Debug.Log("Stopped using");
    }
}
