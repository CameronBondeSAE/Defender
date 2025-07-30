using UnityEngine;

namespace NicholasScripts
{
    public class MGTurret : Turret, IUsableItem, IInteractable
    {
        /*
         * Need to make another script that this will inherit from. Want to
         * try and make a couple turret game objects that have different effects
         *
         * This turret specifically will shoot bullets towards a target - maybe player/civs as well
         */
        public void UseItem()
        {
            throw new System.NotImplementedException();
        }

        public void Interact()
        {
            throw new System.NotImplementedException();
        }

        public void StopInteracting()
        {
            throw new System.NotImplementedException();
        }

        
    }
}
