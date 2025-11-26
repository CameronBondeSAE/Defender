using Defender;
using UnityEngine;

public class LightItem : MonoBehaviour, IUsable
{
    public void Use(CharacterBase characterTryingToUse)
    {
   
        Debug.Log("Trying to use light item "+characterTryingToUse.name);
        
    }

    public void StopUsing()
    {
        Debug.Log("Stopping light");
    }
}
