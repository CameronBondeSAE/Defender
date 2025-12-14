using Unity.Netcode;
using UnityEngine;

public class Persistence : MonoBehaviour
{
    private static Persistence instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
