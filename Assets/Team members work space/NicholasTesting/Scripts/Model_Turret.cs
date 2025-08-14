using Defender;
using UnityEngine;

namespace NicholasScripts
{
    [System.Serializable]
    public class Model_Turret : MonoBehaviour, IUsable, IPickup
    {
        [Header("Fire Rates")]
        public float baseFireRate = 1f;
        public float poweredFireRate = 3f;

        [Header("State")]
        public bool isPowered = false;    // still influences fire rate
        public bool isActivated = false;  

        [Header("Targeting")]
        public float range = 6f; 

        [Header("Runtime")]
        public float fireTimer = 0f;

        public float CurrentFireRate => isPowered ? poweredFireRate : baseFireRate;

        public void UpdateTimer(float deltaTime)
        {
            fireTimer += deltaTime;
        }

        public bool CanFire()
        {
            return fireTimer >= 1f / Mathf.Max(0.0001f, CurrentFireRate);
        }

        public void ResetTimer()
        {
            fireTimer = 0f;
        }

        public void Use(CharacterBase characterTryingToUse)
        {
            isActivated = true;
         
        }

        public void StopUsing()
        {
            isActivated = false;

        }

        public void Pickup(CharacterBase whoIsPickupMeUp)
        {
            StopUsing();

        }

        public void Drop()
        {
            
        }
    }
}