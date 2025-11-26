using System;
using Defender;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class SomethingObj : NetworkBehaviour, IUsable
{ 
    public bool spinning = false;
    public Vector3 rotationSpeed = new Vector3(0f, 100f, 0f);
    public void Use(CharacterBase characterTryingToUse)
    {
        spinning = true;
        Debug.Log("The item is using by:" + characterTryingToUse);
        throw new System.NotImplementedException();
    }

    public void StopUsing()
    {
        spinning = false;
        throw new System.NotImplementedException();
    }

    public void FixedUpdate()
    {
        if (spinning)
        {
            transform.Rotate(rotationSpeed *  Time.deltaTime);
        }
    }
}
