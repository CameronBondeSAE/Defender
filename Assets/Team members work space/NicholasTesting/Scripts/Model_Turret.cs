using Defender;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace NicholasScripts
{
    /// <summary>
    /// Turret MVC model: holds state (power/activate), fire rates, range, and firing timers.
    /// </summary>
    [System.Serializable]
    public class Model_Turret : UsableItem_Base 
    
    {
        [Header("Fire Rates")]
        public float baseFireRate = 1f;
        public float poweredFireRate = 3f;

        [Header("State")]
        public bool isPowered = false;    // still influences fire rate
        public bool isActivated = false;  
        
        // Danni's networked activation to sync sphere of influence color change
        private NetworkVariable<bool> activatedNetVar =
            new NetworkVariable<bool>(
                false,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        [Header("Targeting")]
        public float range = 6f; 

        [Header("Runtime")]
        public float fireTimer = 0f;

        public float CurrentFireRate => isPowered ? poweredFireRate : baseFireRate;
        
        /// <summary>
        /// networked activation
        /// </summary>
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            isActivated = activatedNetVar.Value;
            activatedNetVar.OnValueChanged += OnActivatedChanged;
        }

        public override void OnNetworkDespawn()
        {
            activatedNetVar.OnValueChanged -= OnActivatedChanged;
            base.OnNetworkDespawn();
        }
        
        private void OnActivatedChanged(bool previous, bool current)
        {
            isActivated = current; 
        }

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

        public override void Use(CharacterBase characterTryingToUse)
        {
            base.Use(characterTryingToUse);
            //isActivated = true;
            if (IsServer) activatedNetVar.Value = true;
         
        }

        public override void StopUsing()
        {
            //isActivated = false;
            if (IsServer) activatedNetVar.Value = false;

        }

        public override void Pickup(CharacterBase whoIsPickupMeUp)
        {
            StopUsing();

        }

        public override void Drop()
        {
            
        }


    }
}