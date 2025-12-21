using UnityEngine;

namespace Shell_AI
{
    public class AlienDebugUI : MonoBehaviour
    {
        AlienWait wait;
        AlienCooldown cooldown;

        void Awake()
        {
            wait = GetComponent<AlienWait>();
            cooldown = GetComponent<AlienCooldown>();
        }

        void OnGUI()
        {
            Vector3 pos =
                Camera.main.WorldToScreenPoint(
                    transform.position + Vector3.up * 2f
                );

            if (pos.z < 0) return;

            GUI.Label(
                new Rect(
                    pos.x,
                    Screen.height - pos.y,
                    180,
                    50
                ),
                $"State: {wait.state}\n" +
                $"Cooldown: {cooldown.IsCoolingDown}"
            );
        }
    }
}
