using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NicholasScripts
{
    /// <summary>
    /// Generator MVC model: stores range/usage and tracks powered objects; handles power on/off.
    /// </summary>
    [System.Serializable]
    public class Model_Generator : NetworkBehaviour
    {
        [Header("Config")]
        public float powerRange = 5f;

        [Header("State")]
        public bool isUsed = false;

        private List<IPowerable> poweredObjects = new List<IPowerable>();

        
        private NetworkVariable<bool> usedNetVar =
            new NetworkVariable<bool>(
                false,
                NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            isUsed = usedNetVar.Value;
            usedNetVar.OnValueChanged += OnUsedChanged;
        }

        public override void OnNetworkDespawn()
        {
            usedNetVar.OnValueChanged -= OnUsedChanged;
            base.OnNetworkDespawn();
        }

        private void OnUsedChanged(bool previous, bool current)
        {
            isUsed = current;
            // When synced state changes, we also apply power logic
            if (isUsed)
            {
                ActivateObjects(transform);
            }
            else
            {
                StopAll();
            }
        }

        /// <summary>
        /// Server-only use logic (activates generator + powers objects).
        /// </summary>
        public void Use(Transform generatorTransform)
        {
            if (!IsServer) return;
            if (isUsed) return;

            usedNetVar.Value = true;
            ActivateObjects(generatorTransform);
        }

        private void ActivateObjects(Transform generatorTransform)
        {
            Collider[] hits = Physics.OverlapSphere(generatorTransform.position, powerRange);
            foreach (var hit in hits)
            {
                var powerable = hit.GetComponent<IPowerable>();
                if (powerable != null && !poweredObjects.Contains(powerable))
                {
                    powerable.SetPowered(true);
                    poweredObjects.Add(powerable);
                }
            }
        }

        /// <summary>
        /// Server-only stop logic (turns off generator + powers off objects).
        /// </summary>
        public void StopAll()
        {
            if (!IsServer && NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                return;

            foreach (var powerable in poweredObjects)
            {
                if (powerable != null)
                    powerable.SetPowered(false);
            }

            poweredObjects.Clear();
            isUsed = false;

            if (IsServer) usedNetVar.Value = false;
        }
    }
}
