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
		public string lobbyName      = "Cam's Lobby";
		public int    maxPlayers     = 4;
		public float  heartBeatDelay = 15f;

		public string playerName = "CAM";

		Lobby lobby;

		async void Awake()
		{
			try
			{
				await UnityServices.InitializeAsync();
				SetupAuthenticationEvents();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		public async void CreateLobby()
		{
			Debug.Log("Creating lobby...");
			CreateLobbyOptions options = new CreateLobbyOptions();
			options.IsPrivate = false;

			lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

			await SetupLobbyEvents();

			// Heartbeat the lobby every 15 seconds.
			StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, heartBeatDelay));
		}

		public async void JoinLobbyByID()
		{
			try
			{
				if (lobby != null)
				{
					Debug.Log("Already have a local lobby!");
					return;
				}

				lobby = await LobbyService.Instance.JoinLobbyByIdAsync("Cam's Lobby");
			}
			catch (LobbyServiceException e)
			{
				Debug.Log(e);
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
				foreach (var l in lobbies.Results)
				{
					Debug.Log("---------------------");
					Debug.Log(l.Name);
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

		void SetupAuthenticationEvents()
		{
			// Setup authentication event handlers if desired
			AuthenticationService.Instance.SignedIn += () =>
			                                           {
				                                           // Shows how to get a playerID
				                                           Debug
					                                           .Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

				                                           // Shows how to get an access token
				                                           Debug
					                                           .Log($"Access Token: {AuthenticationService.Instance.AccessToken}");
			                                           };

			AuthenticationService.Instance.SignInFailed += (err) => { Debug.LogError(err); };

			AuthenticationService.Instance.SignedOut += () => { Debug.Log("Player signed out."); };

			AuthenticationService.Instance.Expired += () =>
			                                          {
				                                          Debug
					                                          .Log("Player session could not be refreshed and expired.");
			                                          };
		}

		IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
		{
			while (true)
			{
				Debug.Log("Heartbeating lobby");
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
	}
}