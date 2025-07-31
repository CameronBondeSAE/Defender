using UnityEngine;

namespace NicholasScripts
{
    public abstract class Turret : MonoBehaviour, IPowerable
    {
        /*
         * This will be the base abstract script for the turrets that I plan to create
         */
        public void SetPowered(bool state)
        {
            throw new System.NotImplementedException();
        }
    }
}
