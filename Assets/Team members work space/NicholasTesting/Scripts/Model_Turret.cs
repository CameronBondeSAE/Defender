using UnityEngine;

namespace NicholasScripts
{
    [System.Serializable]
    public class Model_Turret : MonoBehaviour
    {
        public float baseFireRate = 1f;
        public float poweredFireRate = 3f;

        public bool isPowered = false;
        public float fireTimer = 0f;

        public float CurrentFireRate => isPowered ? poweredFireRate : baseFireRate;

        public void UpdateTimer(float deltaTime)
        {
            fireTimer += deltaTime;
        }

        public bool CanFire()
        {
            return fireTimer >= 1f / CurrentFireRate;
        }

        public void ResetTimer()
        {
            fireTimer = 0f;
        }
    }
}