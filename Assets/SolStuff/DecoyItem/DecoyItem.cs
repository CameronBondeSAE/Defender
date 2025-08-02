using UnityEngine;

public class DecoyItem : MonoBehaviour
{
    //when is picked up it tells the player it is a decoy item
    public void Interact()
    {
        Debug.Log("Hey, I'm a Decoy Item :)");
    }
    //when placed down it tells the aliens to get it 
    public void StopInteracting()
    {
        Debug.Log("Hey aliens! Come get me!");
    }

    
}
