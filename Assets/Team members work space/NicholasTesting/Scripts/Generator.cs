using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Defender;
using Unity.Netcode;

namespace NicholasScripts
{
    /// <summary>
    /// Usable generator: after a startup delay, powers nearby IPowerable objects within range.
    /// Networked with Netcode for GameObjects.
    /// </summary>
    public class Generator : UsableItem_Base
    {
        [Header("Generator MVC")]
        public Model_Generator model = new Model_Generator();
        public View_Generator view = new View_Generator();

        private List<IPowerable> poweredObjects = new List<IPowerable>();
        private Coroutine startupCoroutine;

        // Networked state
        private NetworkVariable<bool> netIsUsed = new NetworkVariable<bool>(
            defaultValue: false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public bool IsUsed => netIsUsed.Value;
        public float PowerRange => model.powerRange;

        private void Start()
        {
            model.isUsed = false;

            // Sync state on clients when it changes
            netIsUsed.OnValueChanged += (oldVal, newVal) =>
            {
                model.isUsed = newVal;
                if (newVal)
                {
                    view.PlaySparks();
                    view.PlayRunningLoop();
                }
                else
                {
                    view.StopAudio();
                    view.StopSparks();
                }
            };
        }

        private void Update()
        {
            if (!IsUsed) return;

            if (!IsServer) return; // Server controls powering logic

            // Remove invalid or out-of-range objects
            for (int i = poweredObjects.Count - 1; i >= 0; i--)
            {
                IPowerable powerable = poweredObjects[i];
                if (powerable == null)
                {
                    poweredObjects.RemoveAt(i);
                    continue;
                }

                MonoBehaviour mb = powerable as MonoBehaviour;
                if (mb == null) continue;

                float distance = Vector3.Distance(transform.position, mb.transform.position);
                if (distance > model.powerRange)
                {
                    powerable.SetPowered(false);
                    poweredObjects.RemoveAt(i);
                }
            }

            // Add new objects in range
            Collider[] hits = Physics.OverlapSphere(transform.position, model.powerRange);
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

        public override void Use(CharacterBase characterTryingToUse)
        {
            base.Use(characterTryingToUse);
            if (IsUsed || startupCoroutine != null)
                return;

            // Only server can start generator
            if (IsServer)
            {
                view.PlayStartupSound();
                startupCoroutine = StartCoroutine(ActivateAfterDelay());
            }
            else
            {
                // Tell the server we want to use the generator
                RequestUseServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestUseServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!IsUsed && startupCoroutine == null)
            {
                view.PlayStartupSound();
                startupCoroutine = StartCoroutine(ActivateAfterDelay());
            }
        }

        private IEnumerator ActivateAfterDelay()
        {
            yield return new WaitForSeconds(10f);

            model.isUsed = true;
            netIsUsed.Value = true; // Sync with all clients

            Collider[] hits = Physics.OverlapSphere(transform.position, model.powerRange);
            foreach (var hit in hits)
            {
                var powerable = hit.GetComponent<IPowerable>();
                if (powerable != null && !poweredObjects.Contains(powerable))
                {
                    powerable.SetPowered(true);
                    poweredObjects.Add(powerable);
                }
            }

            startupCoroutine = null;
        }

        public override void StopUsing()
        {
            if (!IsUsed && startupCoroutine == null) return;

            if (IsServer)
                StopGeneratorInternal();
            else
                RequestStopServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void RequestStopServerRpc(ServerRpcParams rpcParams = default)
        {
            StopGeneratorInternal();
        }

        private void StopGeneratorInternal()
        {
            if (startupCoroutine != null)
            {
                StopCoroutine(startupCoroutine);
                startupCoroutine = null;
            }

            foreach (var powerable in poweredObjects)
            {
                if (powerable != null)
                    powerable.SetPowered(false);
            }
            poweredObjects.Clear();

            model.isUsed = false;
            netIsUsed.Value = false; // Sync with all clients
        }

        private new void OnDestroy()
        {
            model.StopAll();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, model.powerRange);
        }

        public override void Pickup(CharacterBase whoIsPickupMeUp)
        {
            if (IsUsed || startupCoroutine != null)
            {
                StopUsing();
            }
        }

        public override void Drop()
        {
        }
    }
}
