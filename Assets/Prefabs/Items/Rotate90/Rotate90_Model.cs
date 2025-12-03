using Defender;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class Rotate90_Model : NetworkBehaviour, IUsable
{
	public RotateView view;

    public void Use(CharacterBase characterTryingToUse)
    {
	    Debug.Log(name + " : "+ characterTryingToUse.name + " is using");
	    transform.DORotate(new Vector3(0,transform.eulerAngles.y+90f,0), 2f, RotateMode.Fast);
	    view.stoneSound_RPC();
	    view.stoneEmitRight_RPC();
	    view.stoneEmitLeft_RPC();
    }

    public void StopUsing()
    {
	    Debug.Log(name + " stopped using");
    }
}
