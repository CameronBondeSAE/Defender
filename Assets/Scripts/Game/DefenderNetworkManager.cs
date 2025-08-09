using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class DefenderNetworkManager : NetworkManager
{
	public void StartLocalHostForTesting()
	{
		UnityTransport transport = NetworkConfig.NetworkTransport as UnityTransport;
		
		transport.SetConnectionData("127.0.0.1", 7777);
		Debug.Log($"Protocol after SetConnectionData: {transport.Protocol}");
		StartHost();
	}

	public void JoinLocalHostForTesting()
	{
		UnityTransport transport = NetworkConfig.NetworkTransport as UnityTransport;
		
		transport.SetConnectionData("127.0.0.1", 7777);
		Debug.Log($"Protocol after SetConnectionData: {transport.Protocol}");
		
		StartClient();
	}
}
