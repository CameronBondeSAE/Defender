    using System.Collections;
    using UnityEngine;
    using System.Collections.Generic;
    using Defender;

    namespace NicholasScripts
    {
        /// <summary>
        /// Usable generator: after a startup delay, powers nearby IPowerable objects within range.
        /// </summary>
        public class Generator : UsableItem_Base
        {
            [Header("Generator MVC")]
            public Model_Generator model = new Model_Generator();
            public View_Generator view = new View_Generator();

            private List<IPowerable> poweredObjects = new List<IPowerable>();
            private Coroutine startupCoroutine;
            
            public bool IsUsed => model.isUsed;
            public float PowerRange => model.powerRange;

            private void Start()
            {
                model.isUsed = false;
            }

            private void Update()
            {
                if (!model.isUsed) return;

                for (int i = poweredObjects.Count - 1; i >= 0; i--)
                {
                    IPowerable powerable = poweredObjects[i];
                    if (powerable == null) 
                    {
                        poweredObjects.RemoveAt(i);
                        continue;
                    }

                    MonoBehaviour mb = powerable as MonoBehaviour;
                    if (mb == null) continue; // Skip if not a MonoBehaviour

                    float distance = Vector3.Distance(transform.position, mb.transform.position);
                    if (distance > model.powerRange)
                    {
                        //Debug.Log($"Object {mb.name} left range — powering off.");
                        powerable.SetPowered(false);
                        poweredObjects.RemoveAt(i);
                    }
                }

                Collider[] hits = Physics.OverlapSphere(transform.position, model.powerRange);
                foreach (var hit in hits)
                {
                    var powerable = hit.GetComponent<IPowerable>();
                    if (powerable != null && !poweredObjects.Contains(powerable))
                    {
                        //Debug.Log($"Object {hit.name} entered range — powering on.");
                        powerable.SetPowered(true);
                        poweredObjects.Add(powerable);
                    }
                }
            }

            public override void Use(CharacterBase characterTryingToUse)
            {
                base.Use(characterTryingToUse);
                if (model.isUsed || startupCoroutine != null)
                {
                    //Debug.Log("Generator already used or starting.");
                    return;
                }

                //Debug.Log("Starting generator...");
                view.PlayStartupSound();

                startupCoroutine = StartCoroutine(ActivateAfterDelay());
            }

            private IEnumerator ActivateAfterDelay()
            {
                yield return new WaitForSeconds(10f);

                //Debug.Log("Generator activated.");
                model.isUsed = true;
                view.PlaySparks();

                Collider[] hits = Physics.OverlapSphere(transform.position, model.powerRange);
                //Debug.Log($"Found {hits.Length} colliders in range.");
                foreach (var hit in hits)
                {
                    var powerable = hit.GetComponent<IPowerable>();
                   // Debug.Log($"Hit: {hit.name}, Found powerable: {powerable != null}");
                    if (powerable != null && !poweredObjects.Contains(powerable))
                    {
                        //Debug.Log($"Powering object: {hit.name}");
                        powerable.SetPowered(true);
                        poweredObjects.Add(powerable);
                    }
                }

                view.PlayRunningLoop();
                startupCoroutine = null;
            }
            
            public override void StopUsing()
            {
                if (!model.isUsed && startupCoroutine == null) return;

               // Debug.Log("Stopping generator.");

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

                view.StopAudio();
                view.StopSparks();
            }



            private new void OnDestroy()
            {
                model.StopAll();
            }

            private void OnDrawGizmosSelected()
            {
	            Gizmos.color = Color.yellow;
	            Gizmos.DrawWireSphere(transform.position, model.powerRange);

                // if (view != null)
                    // view.DrawPowerRange(transform.position, model.powerRange);
            }

            public override void Pickup(CharacterBase whoIsPickupMeUp)
            {
                if (model.isUsed || startupCoroutine != null)
                {
                    StopUsing();
                }
            }

            public override void Drop()
            {
            }
        }
    }