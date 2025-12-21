using UnityEngine;

namespace Shell_AI
{
    public class AlienCooldown : MonoBehaviour
    {
        public float cooldownDuration = 3f;

        private float timer;
        private bool coolingDown;

        public bool IsCoolingDown => coolingDown;

        public void StartCooldown()
        {
            coolingDown = true;
            timer = cooldownDuration;
            Debug.Log("Cooldown started");
        }

        void Update()
        {
            if (!coolingDown) return;

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                coolingDown = false;
                Debug.Log("Cooldown finished");
            }
        }
    }
}
