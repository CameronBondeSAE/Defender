using Unity.Netcode;
using UnityEngine;

namespace AshleyPearson
{

    public class BillboardText : MonoBehaviour
    {
        private void LateUpdate()
        {
            //Not networking as cameras will be different for each client
            //So should be dealt with locally for each client

            if (Camera.main != null)
            {
                transform.LookAt(Camera.main.transform.position);
                transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
            }

            else
            {
                Debug.LogWarning("[Billboard] Main Camera is not assigned.");
            }
        }
    }
}
