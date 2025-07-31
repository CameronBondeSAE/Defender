using UnityEngine;
using System.Collections.Generic;

namespace NicholasScripts
{
    public class Generator : MonoBehaviour, IUsable
    {
        /*
         * This is for the generator item/object that will be used to buff the other select objects
         *
         * To Do:
         * Add Sound played (maybe some sort of generator sparking, or just running) while active
         */
        [System.Serializable]
        public class GeneratorModel
        {
            public float powerRange = 5f;
            public bool isUsed = false;
        }

        [System.Serializable]
        public class GeneratorView
        {
            public void DrawPowerRange(Vector3 position, float range)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(position, range);
            }
        }

        [Header("Generator MVC")]
        public GeneratorModel model = new GeneratorModel();
        public GeneratorView view = new GeneratorView();

        private List<IPowerable> poweredObjects = new List<IPowerable>();

        public void Use()
        {
            if (model.isUsed) return;
            model.isUsed = true;

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

        public void StopUsing()
        {
	        
        }

        private void OnDestroy()
        {
            foreach (var powerable in poweredObjects)
            {
                if (powerable != null)
                    powerable.SetPowered(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (view != null)
                view.DrawPowerRange(transform.position, model.powerRange);
        }
    }
}
