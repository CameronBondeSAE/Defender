using UnityEngine;

namespace adamsScripts
{
    /// <summary>
    /// script for the mind control grenade
    /// </summary>
    public class MindControlGrenade : MonoBehaviour //, IUsable
    {
        [SerializeField] private float explosionRadius;
        // inherits from base grenade/item script? - would need to be picked up, activated/thrown/rolled
        //public void UseItem()
        //{
            // throw grenade in facing direction
            // timer starts
            // explode - in certain radius
        //}
        
        // ACTIVATE
        // starts 5 second timer, after 5 seconds EXPLODE
        
        // THROW
        // adds force in facing direction
        
        private void Explode()
        {
            // play sound
            // particle effects
            
            // check for radius around grenade
            Collider[] charactersHit = Physics.OverlapSphere(transform.position, explosionRadius);
            // change AI of all character bases in radius - ?after x amount of time, revert to previous AI state?
            if (charactersHit.Length > 0)
            {
                foreach (Collider character in charactersHit)
                {
                    // if(character.GetComponent<>()) what common component do aliens & civilians have in common that I can check for?
                }
            }
        }
        
        
        
        
        // after launched will need to explode, have explosion radius, change ai of aliens hit - maybe even civilians?, play sound, spawn particles
    }
}
