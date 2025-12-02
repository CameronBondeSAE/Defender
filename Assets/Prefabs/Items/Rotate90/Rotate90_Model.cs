using Defender;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class Rotate90_Model : NetworkBehaviour, IUsable
{

    public void Use(CharacterBase characterTryingToUse)
    {
	    Debug.Log(name + " : "+ characterTryingToUse.name + " is using");
	    transform.DORotate(new Vector3(0,transform.eulerAngles.y+90f,0), 2f, RotateMode.Fast);
    }

    public void StopUsing()
    {
	    Debug.Log(name + " stopped using");
    }
}
