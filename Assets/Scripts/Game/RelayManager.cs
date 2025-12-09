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
using UnityEngine.UI;

public class RelayManager : MonoBehaviour
{
	public string         joinCode;
	public TMP_InputField joinCodeInput;
	public TMP_InputField joinCodeDisplay;
	public Button         startHostButton;
	public Button         startClientButton;

	public string reservedRelayJoinCode;
	public Allocation reservedRelayAllocation;
	
	public void ActivateButtons()
	{
		// joinCodeInput.interactable   = true;
		// joinCodeDisplay.interactable = true;
		// startHostButton.interactable  = true;
		// startClientButton.interactable = true;
	}

	private void DeactivateButtons()
	{
	// 	joinCodeInput.interactable   = false;
	// 	joinCodeDisplay.interactable = false;
	// 	startHostButton.interactable  = false;
		// startClientButton.interactable = false;
	}

	async void Start()
	{
		DeactivateButtons();
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
		// Debug.Log("New joincode entered by user = " + joinCode);
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
		// await UnityServices.InitializeAsync();
		// if (!AuthenticationService.Instance.IsSignedIn)
		// {
		// 	await AuthenticationService.Instance.SignInAnonymouslyAsync();
		// }
		//
		Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
		NetworkManager.Singleton.GetComponent<UnityTransport>()
		              .SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
		joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
		Debug.Log(joinCode);
		joinCodeDisplay.text = joinCode; // HACK
		if (NetworkManager.Singleton.StartHost())
		{
			// OnJoinCodeGenerated_Event?.Invoke(joinCode);
			
			return joinCode;
		}
		else
		{
			return null;
		}
	}

	public async Task<string> StartHostWithReservedRelay(string connectionType)
	{
		if (reservedRelayAllocation == null)
		{
			Debug.LogError("RelayManager: No reserved allocation to start host with");
			return null;
		}
		
		NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(reservedRelayAllocation, connectionType));

		if (NetworkManager.Singleton.StartHost())
		{
			joinCode = reservedRelayJoinCode;
			Debug.Log("Host Relay Join Code (reserved) : " + joinCode);
			return joinCode;
		}

		else
		{
			Debug.LogError("RelayManager: Failed to start host with reserved relay code");
			return null;
		}
	}

	public void NewJoinCodeSet(string _relayJoinCode)
	{
		joinCode = _relayJoinCode;
		// ActivateButtons();
	}

	public void StartClientWithJoinCode()
	{
		Debug.Log("StartClientWithJoinCode: " + joinCode);
		StartClientWithRelay(joinCode, "udp");
	}

	public async Task<bool> StartClientWithRelay(string joinCode, string connectionType)
	{
		Debug.Log("StartClientWithRelay: " + joinCode);
		// await UnityServices.InitializeAsync();
		// Debug.Log("Initialised");
		// if (!AuthenticationService.Instance.IsSignedIn)
		// {
		// 	await AuthenticationService.Instance.SignInAnonymouslyAsync();
		// 	Debug.Log("SignedIn Anonymously");
		// }

		Debug.Log("Attempt to join allocation : Joincode = " + joinCode);
		JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
		Debug.Log("Joined! : AllocationId = " + allocation.AllocationId);
		NetworkManager.Singleton.GetComponent<UnityTransport>()
		              .SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
		return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
	}
	
	//Separating out lobby and relay allocation for lobby
	public async Task GetRelayCode()
	{
		reservedRelayAllocation = await RelayService.Instance.CreateAllocationAsync(4); //max players is 4
		reservedRelayJoinCode = await RelayService.Instance.GetJoinCodeAsync(reservedRelayAllocation.AllocationId);
	}
	
	
}