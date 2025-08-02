    using UnityEngine;
    using System.Collections.Generic;

    namespace NicholasScripts
    {
        public class Generator : MonoBehaviour, IUsable
        {
            [Header("Generator MVC")]
            public Model_Generator model = new Model_Generator();
            public View_Generator view = new View_Generator();

            private void Start()
            {
                model.isUsed = false;
            }

            public void Use()
            {
                model.Use(transform);
                view.PlayUseEffect();
            }

            public void StopUsing()
            {
                model.StopAll();
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