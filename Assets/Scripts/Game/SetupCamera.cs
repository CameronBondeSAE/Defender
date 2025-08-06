using Unity.Cinemachine;
using UnityEngine;

public class SetupCamera : MonoBehaviour
{
	void Start()
	{
		var vcam = GetComponentInChildren<CinemachineCamera>();
		vcam.Follow = GetComponentInChildren<PlayerInputHandler2>().transform; // or your camera anchor
		vcam.LookAt = GetComponentInChildren<PlayerInputHandler2>().transform;
	}
}
