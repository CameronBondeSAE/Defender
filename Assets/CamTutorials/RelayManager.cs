using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
	public string         joinCode;
	public TMP_InputField joinCodeInput;
	public TMP_InputField joinCodeDisplay;

	async void Start()
	{
		// Task<string> startHostWithRelay = StartHostWithRelay(4, "udp");

		// Debug.Log(await startHostWithRelay);
	}

	private void OnEnable()
	{
		joinCodeInput.onValueChanged.AddListener(OnSubmit);
	}

	private void OnDisable()
	{
		joinCodeInput.onValueChanged.RemoveListener(OnSubmit);
	}

	private void OnSubmit(string _joinCode)
	{
		joinCode = _joinCode;
		Debug.Log("New joincode entered by user = " + joinCode);
	}

	public void GetRoundTripTime()
	{
		ulong ping = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.Singleton.LocalClientId);
		Debug.Log("Ping to server = "+ping);
	}

	// This is just a middleman for UI buttons/events etc
	public void InitialiseHostWithRelay()
	{
		StartHostWithRelay(4, "udp");
	}

	public async Task<string> StartHostWithRelay(int maxConnections, string connectionType)
	{
		await UnityServices.InitializeAsync();
		if (!AuthenticationService.Instance.IsSignedIn)
		{
			await AuthenticationService.Instance.SignInAnonymouslyAsync();
		}

		Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
		NetworkManager.Singleton.GetComponent<UnityTransport>()
		              .SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
		joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
		Debug.Log(joinCode);
		joinCodeDisplay.text = joinCode; // HACK
		if (NetworkManager.Singleton.StartHost())
		{
			return joinCode;
		}
		else
		{
			return null;
		}
	}

	public void StartClientWithJoinCode()
	{
		Debug.Log("StartClientWithJoinCode: " + joinCode);
		StartClientWithRelay(joinCode, "udp");
	}

	public async Task<bool> StartClientWithRelay(string joinCode, string connectionType)
	{
		Debug.Log("StartClientWithRelay: " + joinCode);
		await UnityServices.InitializeAsync();
		Debug.Log("Initialised");
		if (!AuthenticationService.Instance.IsSignedIn)
		{
			await AuthenticationService.Instance.SignInAnonymouslyAsync();
			Debug.Log("SignedIn Anonymously");
		}

		Debug.Log("Attempt to join allocation : Joincode = " + joinCode);
		JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
		Debug.Log("Joined! : AllocationId = " + allocation.AllocationId);
		NetworkManager.Singleton.GetComponent<UnityTransport>()
		              .SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
		return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
	}
}