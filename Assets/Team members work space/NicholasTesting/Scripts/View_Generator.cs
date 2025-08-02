using UnityEngine;

namespace NicholasScripts
{
    [System.Serializable]
    public class View_Generator : MonoBehaviour
    {
        public void DrawPowerRange(Vector3 position, float range)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, range);
        }

        public void PlayUseEffect()
        {
            // Placeholder for a spark or hum animation/sound
            Debug.Log("Playing generator use effect.");
        }
    }
}