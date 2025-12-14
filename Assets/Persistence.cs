using Unity.Netcode;
using UnityEngine;

public class Persistence : NetworkBehaviour
{
    void Update()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
