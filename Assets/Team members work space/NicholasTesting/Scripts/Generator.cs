    using System.Collections;
    using UnityEngine;
    using System.Collections.Generic;

    namespace NicholasScripts
    {
        public class Generator : MonoBehaviour, IUsable
        {
            [Header("Generator MVC")]
            public Model_Generator model = new Model_Generator();
            public View_Generator view = new View_Generator();

            private List<IPowerable> poweredObjects = new List<IPowerable>();
            private Coroutine startupCoroutine;
            
            private void Start()
            {
                model.isUsed = false;
            }

            public void Use()
            {
                if (model.isUsed || startupCoroutine != null)
                {
                    Debug.Log("Generator already used or starting.");
                    return;
                }

                Debug.Log("Starting generator...");
                view.PlayStartupSound();

                startupCoroutine = StartCoroutine(ActivateAfterDelay());
            }

            private IEnumerator ActivateAfterDelay()
            {
                yield return new WaitForSeconds(10f);

                Debug.Log("Generator activated.");
                model.isUsed = true;
                view.PlaySparks();

                Collider[] hits = Physics.OverlapSphere(transform.position, model.powerRange);
                Debug.Log($"Found {hits.Length} colliders in range.");
                foreach (var hit in hits)
                {
                    var powerable = hit.GetComponent<IPowerable>();
                    Debug.Log($"Hit: {hit.name}, Found powerable: {powerable != null}");
                    if (powerable != null && !poweredObjects.Contains(powerable))
                    {
                        Debug.Log($"Powering object: {hit.name}");
                        powerable.SetPowered(true);
                        poweredObjects.Add(powerable);
                    }
                }

                view.PlayRunningLoop();
                startupCoroutine = null;
            }
            
            public void StopUsing()
            {
                if (!model.isUsed && startupCoroutine == null) return;

                Debug.Log("Stopping generator.");

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



            private void OnDestroy()
            {
                model.StopAll();
            }

            private void OnDrawGizmosSelected()
            {
                if (view != null)
                    view.DrawPowerRange(transform.position, model.powerRange);
            }
        }
    }