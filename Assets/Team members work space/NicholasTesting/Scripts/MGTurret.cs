using UnityEngine;

namespace NicholasScripts
{
    public class MGTurret : Turret, IUsable
    {
        /*
                 * Need to make another script that this will inerit from. Want to
                 * try and make a couple turret game objects that have different effects
                 * This turret specifically will shoot bullets towards a target - maybe player/civs as well
                 */
        public void Use()
        {
            throw new System.NotImplementedException();
        }

        public void StopUsing()
        {
	        
        }

    }
}
