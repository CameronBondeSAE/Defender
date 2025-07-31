using UnityEngine;

public class DecoyItem : MonoBehaviour, IUsable
{
    //when is picked up it tells the player it is a decoy item


    public void Use()
    {
        Debug.Log("Hey, I'm a Decoy Item :)");
    }

    public void StopUsing()
    {
        Debug.Log("Hey aliens! Come get me!");
    }
}
