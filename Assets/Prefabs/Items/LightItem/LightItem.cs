using System.Collections;
using Defender;
using UnityEngine;

public class LightItem : MonoBehaviour, IUsable, IPowerable
{
    public Light spotLight;
    private IEnumerator timeCoroutine;
    public void Use(CharacterBase characterTryingToUse)
    {
        
        Debug.Log("Trying to use light item "+characterTryingToUse.name);
        
        spotLight.enabled = !spotLight.enabled;
        
    }
    // write down and make the light a toggle, as well as implement a timer for the light to turn off after some time

    
    public void StopUsing()
    {
        
        Debug.Log("Stopping light");
        
    }

    public void SetPowered(bool powered)
    {
        
    }
    
}
