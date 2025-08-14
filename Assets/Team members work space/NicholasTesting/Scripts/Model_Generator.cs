using System.Collections.Generic;
using UnityEngine;

namespace NicholasScripts
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class Model_Generator : MonoBehaviour
    {
        public float powerRange = 5f;
        public bool isUsed = false;
        private List<IPowerable> poweredObjects = new List<IPowerable>();

        public void Use(Transform generatorTransform)
        {
            if (isUsed)
            {
               // Debug.Log("Generator already used.");
                return;
            }

           // Debug.Log("Generator activated.");
            isUsed = true;

            Collider[] hits = Physics.OverlapSphere(generatorTransform.position, powerRange);
            //Debug.Log($"Found {hits.Length} colliders in range.");
            foreach (var hit in hits)
            {
                var powerable = hit.GetComponent<IPowerable>();
                //Debug.Log($"Hit: {hit.name}, Found powerable: {powerable != null}");
                if (powerable != null && !poweredObjects.Contains(powerable))
                {
                   // Debug.Log($"Powering object: {hit.name}");
                    powerable.SetPowered(true);
                    poweredObjects.Add(powerable);
                }
            }
        }

        public void StopAll()
        {
            foreach (var powerable in poweredObjects)
            {
                if (powerable != null)
                    powerable.SetPowered(false);
            }

            poweredObjects.Clear();
        }
        
    }
}