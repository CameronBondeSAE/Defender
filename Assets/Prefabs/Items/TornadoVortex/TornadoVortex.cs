using Defender;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class TornadoVortex : UsableItem_Base
{
    public override void Use(CharacterBase characterTryingToUse)
    {
        base.Use(characterTryingToUse);
		
        Debug.Log("Use GetTiny_Model : By "+characterTryingToUse.name);
        
    }
    public override void Pickup(CharacterBase whoIsPickupMeUp)
    {
        base.Pickup(whoIsPickupMeUp);
		
        Debug.Log("Tornado Item picked up by : "+whoIsPickupMeUp.name);
    }
}
