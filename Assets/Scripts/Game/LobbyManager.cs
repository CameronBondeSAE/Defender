using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CameronBonde
{
	public class LobbyManager : MonoBehaviour
	{
		public string lobbyName      = "Defender Lobby #1";
		public int    maxPlayers     = 4;
		public float  heartBeatDelay = 15f;

		public string playerName = "CAM";
		
		public RelayManager relayManager;
		public AuthenticationManager authenticationManager;
		
		Lobby lobby;

		private void Start()
		{
			UnityServices.InitializeAsync();
		}

		// private void OnEnable()
		// {
		// 	authenticationManager.OnSignedIn += CreateLobby;
		// }
		//
		// private void OnDisable()
		// {
		// 	authenticationManager.OnSignedIn -= CreateLobby;
		// }
		
		public async void CreateLobby()
		{
			Debug.Log("Creating lobby...");
			
			await authenticationManager.SignInAsync();

			await relayManager.StartHostWithRelay(maxPlayers, "udp");
			
			CreateLobbyOptions options = new CreateLobbyOptions();
			options.IsPrivate = false;
			options.IsLocked  = false;
			options.Data      = new Dictionary<string, DataObject>();
			options.Data.Add("RelayJoinCode", new DataObject(
			                                                 visibility: DataObject.VisibilityOptions.Member,
			                                                 value: relayManager.joinCode));
			
			lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

			// await SetupLobbyEvents();
			
			// await SetAllLobbyData();

			// Heartbeat the lobby every 15 seconds.
			StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, heartBeatDelay));
		}

		public async void JoinLobbyByCode(string lobbyCode)
		{
			Debug.Log("Joining lobby...");
			await authenticationManager.SignInAsync();

			try
			{
				if (lobby != null)
				{
					Debug.Log("Already have a local lobby!");
					return;
				}

				Debug.Log("Lobby code = " + lobbyCode);
				lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyCode); // TODO create custom name. This is just one lobby ever
				
				lobby.Data.TryGetValue("RelayJoinCode", out DataObject relayJoinCode);
				if (relayJoinCode != null)
				{
					relayManager.NewJoinCodeSet(relayJoinCode.Value);
					relayManager.StartClientWithJoinCode();
				}
			}
			catch (LobbyServiceException e)
			{
				Debug.LogError($"Failed to join lobby: {e}");
			}
		}
		
		public async void JoinFirstAvailableLobby()
		{
			Debug.Log("Joining first available lobby...");
			await authenticationManager.SignInAsync();

			QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(
			                          new QueryLobbiesOptions
			                          {
				                          Count = 1,
				                          Filters = new List<QueryFilter>
				                                    {
					                                    new QueryFilter(QueryFilter.FieldOptions.Name, lobbyName, QueryFilter.OpOptions.EQ),
					                                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
				                                    }
			                          });
    
			if (response.Results.Count > 0)
			{
				lobby = await LobbyService.Instance.JoinLobbyByIdAsync(response.Results[0].Id);

				lobby.Data.TryGetValue("RelayJoinCode", out DataObject relayJoinCode);
				if (relayJoinCode != null)
				{
					relayManager.NewJoinCodeSet(relayJoinCode.Value);
					relayManager.StartClientWithJoinCode();
				}
			}
		}
		
		private void OnLobbyChanged(ILobbyChanges obj)
		{
			Debug.Log("Lobby changed event");
			if (obj.Data.Value != null)
				foreach (var changedOrRemovedLobbyValue in obj.Data.Value)
				{
					Debug.Log(changedOrRemovedLobbyValue);
				}
		}

		public async void SetRandomPlayerName()
		{
			try
			{
				UpdatePlayerOptions options = new UpdatePlayerOptions();

				options.Data = new Dictionary<string, PlayerDataObject>();
				options.Data.Add("PlayerName", new PlayerDataObject(
				                                                      visibility: PlayerDataObject.VisibilityOptions.Public,
				                                                      value: "Cam's clone number "+Random.Range(0,1000)));

				//Ensure you sign-in before calling Authentication Instance
				//See IAuthenticationService interface
				string playerId = AuthenticationService.Instance.PlayerId;

				await LobbyService.Instance.UpdatePlayerAsync(this.lobby.Id, playerId, options);

				
			}
			catch (LobbyServiceException e)
			{
				Debug.Log(e);
			}
		}

		public async void QueryLobbies()
		{
			try
			{
				QueryLobbiesOptions options = new QueryLobbiesOptions();
				options.Count = 25;

				// Filter for open lobbies only
				options.Filters = new List<QueryFilter>()
				                  {
					                  new QueryFilter(
					                                  field: QueryFilter.FieldOptions.AvailableSlots,
					                                  op: QueryFilter.OpOptions.GT,
					                                  value: "0")
				                  };

				// Order by newest lobbies first
				options.Order = new List<QueryOrder>()
				                {
					                new QueryOrder(
					                               asc: false,
					                               field: QueryOrder.FieldOptions.Created)
				                };

				QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);

				// TODO: UI
				Debug.Log("Found " + lobbies.Results.Count + " lobbies");
				foreach (Lobby l in lobbies.Results)
				{
					Debug.Log("---------------------");
					Debug.Log(l.Name);
					if (l.Data != null)
						foreach (KeyValuePair<string, DataObject> dataObject in l.Data)
						{
							if (dataObject.Key == "RelayJoinCode")
							{
								Debug.Log("RelayJoinCode : " + dataObject.Value.Value);
							}

							Debug.Log(dataObject.Key + " : " + dataObject.Value.Value);
						}

					foreach (Player p in l.Players)
					{
						if (p.Data != null)
						{
							foreach (KeyValuePair<string, PlayerDataObject> playerDataObject in p.Data)
							{
								Debug.Log(playerDataObject.Key + " : " + playerDataObject.Value.Value);
							}
						}

						else
						{
							Debug.Log("No player data found");
						}
					}

					Debug.Log("^^^^^^^^^^^^^^^^^^^^^");
				}
			}
			catch (LobbyServiceException e)
			{
				Debug.Log(e);
			}
		}

		private async Task SetupLobbyEvents()
		{
			Debug.Log("Setting up Lobby events");
			var callbacks = new LobbyEventCallbacks();
			callbacks.LobbyChanged += OnLobbyChanged;
			callbacks.DataChanged += CallbacksOnDataChanged;

			// callbacks.KickedFromLobby                  += OnKickedFromLobby;
			// callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;

			try
			{
				ILobbyEvents m_LobbyEvents =
					await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
			}
			catch (LobbyServiceException ex)
			{
				switch (ex.Reason)
				{
					case LobbyExceptionReason.AlreadySubscribedToLobby:
						Debug.LogWarning($"Already subscribed to lobby[{lobby.Id}]. We did not need to try and subscribe again. Exception Message: {ex.Message}");
						break;
					case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy:
						Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}");
						throw;
					case LobbyExceptionReason.LobbyEventServiceConnectionError:
						Debug.LogError($"Failed to connect to lobby events. Exception Message: {ex.Message}");
						throw;
					default: throw;
				}
			}
		}

		private void CallbacksOnDataChanged(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> obj)
		{
			// Debug.Log("Data changed event : " + obj);
			
			// TODO more than just relaycode, presumably lobby name changes etc
			if (obj.TryGetValue("RelayJoinCode", out ChangedOrRemovedLobbyValue<DataObject> relayJoinCode))
			{
				relayManager.NewJoinCodeSet(relayJoinCode.Value.Value);
			}
		}
		
		IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
		{
			while (true)
			{
				// Debug.Log("Heartbeating lobby "+Random.Range(0,1000));
				LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
				yield return new WaitForSecondsRealtime(waitTimeSeconds);
			}

			yield return null;
		}

		// TODO: Delete lobbies on exit. Unity demo code for multiple lobbies
		// ConcurrentQueue<string> createdLobbyIds = new ConcurrentQueue<string>();
		//
		// void OnApplicationQuit()
		// {
		// 	while (createdLobbyIds.TryDequeue(out var lobbyId))
		// 	{
		// 		LobbyService.Instance.DeleteLobbyAsync(lobbyId);
		// 	}
		// }

		private void OnApplicationQuit()
		{
			if (lobby != null) LobbyService.Instance.DeleteLobbyAsync(lobby.Id);
		}

		/// <summary>
		/// Brute force, rewrites ALL lobby data in one go. Yes you can selectively update a simple key, but this is clearer for now
		/// </summary>
		/// <param name="_joinCode"></param>
		public async Task SetAllLobbyData()
		{
			Debug.Log("Trying to set all lobby data");
			try
			{
				UpdateLobbyOptions options = new UpdateLobbyOptions();
				options.Name       = lobbyName;
				options.MaxPlayers = maxPlayers;
				options.IsPrivate  = false;

				//Ensure you sign-in before calling Authentication Instance
				//See IAuthenticationService interface
				options.HostId = AuthenticationService.Instance.PlayerId;
				
				options.Data = new Dictionary<string, DataObject>();
				options.Data.Add("RelayJoinCode", new DataObject(
				                                                      visibility: DataObject.VisibilityOptions.Private,
				                                                      value: relayManager.joinCode));
				await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, options);
				Debug.Log("Updated lobby data");
			}
			catch (LobbyServiceException e)
			{
				Debug.Log(e);
			}
		}
	}
}