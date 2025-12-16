using UnityEngine;

namespace Shell_AI
{
    public class AlienSense : MonoBehaviour
    {
        public float senseRadius = 6f;

        public bool seesPlayer;
        public bool seesCivilian;
        public int nearbyAliens;

        void Update()
        {
            seesPlayer = false;
            seesCivilian = false;
            nearbyAliens = 0;

            Collider[] hits =
                Physics.OverlapSphere(
                    transform.position,
                    senseRadius
                );

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                    seesPlayer = true;

                if (hit.CompareTag("Civilian"))
                    seesCivilian = true;

                if (hit.CompareTag("Alien"))
                    nearbyAliens++;
            }
        }
    }
}
