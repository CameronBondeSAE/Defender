using System;
using Unity.Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraManager : MonoBehaviour
{
	public static CameraManager instance; // This is a hack


	public CinemachineVirtualCamera virtualCamera;

	public bool test;


	private void Awake()
	{
		instance = this;
	}
}
