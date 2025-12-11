using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AshleyPearson;
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
		public string playerName = "CAM";
		public int    maxPlayers     = 4;
		public float  heartBeatDelay = 15f;
		
		public RelayManager relayManager;
		public AuthenticationManager authenticationManager;
		
		public Lobby lobby;
		
		//Added name variables
		public string inputLobbyName;
		public string inputUsername;
		
		public PlayerName playerNameScript;
		
		
		private async void Start()
		{
			//Edits: Switched function to async as this was causing issues with lobby joining.
			
			while (!authenticationManager.AreServicesInitialized() && Application.isPlaying)
			{
				await Task.Yield();
				// Debug.Log("LobbyManager: Services initialized. Lobby Manager can act now.");
			}
			
			//This is being called in authentication manager already.
			//UnityServices.InitializeAsync(); // BUG: This MAY happen AFTER create lobby is called, because it's not async
		}

		 private void OnEnable()
		{
			//authenticationManager.OnSignedIn += CreateLobby;
			LobbyEvents.OnButtonClicked_HostGame += CreateLobbyForBrowser_ButtonWrapper;
			LobbyEvents.OnButtonClicked_JoinGame += JoinLobbyForBrowser_ButtonWrapper;
		}
		
		private void OnDisable()
		{
		// 	authenticationManager.OnSignedIn -= CreateLobby;
			LobbyEvents.OnButtonClicked_HostGame -= CreateLobbyForBrowser_ButtonWrapper;
			LobbyEvents.OnButtonClicked_JoinGame -= JoinLobbyForBrowser_ButtonWrapper;
		}
		
		public async void CreateLobby(string inputLobbyName)
		{
			Debug.Log("Creating lobby... Name = "+inputLobbyName);
			
			await authenticationManager.SignInAsync();
		
			await relayManager.StartHostWithRelay(maxPlayers, "udp");
			
			CreateLobbyOptions options = new CreateLobbyOptions();
			options.IsPrivate = false;
			options.IsLocked  = false;
			options.Data      = new Dictionary<string, DataObject>();
			options.Data.Add("RelayJoinCode", new DataObject(
			                                                 visibility: DataObject.VisibilityOptions.Public,
			                                                 value: relayManager.joinCode));
			
			lobby = await LobbyService.Instance.CreateLobbyAsync(inputLobbyName, maxPlayers, options);
			Debug.Log("LobbyManager: Lobby name is " + lobby.Name);
			Debug.Log("LobbyManager: Lobby join code is " + lobby.LobbyCode);
			Debug.Log("LobbyManager: Relay join code is " + relayManager.joinCode);
		
			//Save Player Name
			await SetPlayerUsername(lobby);
			
			//Save lobby join code
			await SetLobbyJoinCode(lobby);
			
			// await SetupLobbyEvents();
			// await SetAllLobbyData();
		
			// Heartbeat the lobby every 15 seconds.
			StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, heartBeatDelay));
			
			// HACK CAM
			FindFirstObjectByType<LevelLoader>().LoadFirstLevelServerRpc();
		}

		public async void CreateLobbyForBrowser_ButtonWrapper(string inputLobbyName)
		{
			CreateLobbyForBrowser(inputLobbyName);
		}
		
		//This version doesn't connect the relay straight away so that players can join the lobby
		public async Task CreateLobbyForBrowser(string inputLobbyName)
		{
			Debug.Log("Creating lobby... but not starting network yet");
			
			await authenticationManager.SignInAsync();

			// HACK: TOO EARLY IT GOES STALE BEFORE STARTING
			// await relayManager.GetRelayCode();
			
			CreateLobbyOptions options = new CreateLobbyOptions();
			options.IsPrivate = false;
			options.IsLocked  = false;
			options.Data      = new Dictionary<string, DataObject>();
			
			lobby = await LobbyService.Instance.CreateLobbyAsync(inputLobbyName, maxPlayers, options);
			
			//Set up callbacks
			await SetupLobbyEvents();
			
			//Update the lobby data class with relevant info
			await InitialLobbyUpdate(lobby);

			// Heartbeat the lobby every 15 seconds.
			StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, heartBeatDelay));
			
			//Call to wait for players
			LobbyEvents.WaitingForOtherPlayersToJoinLobby?.Invoke(lobby.Players.Count);
		}

		public async void JoinLobbyByCode(string lobbyCode)
		{
			Debug.Log("LobbyManager: Join code is: " +  lobbyCode);
			
			Debug.Log("Joining lobby...");
			await authenticationManager.SignInAsync();
			
			lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
			
			//Set joining player's username
			await SetPlayerUsername(lobby);
			
			try
			{
				//Get relay join code and start client
				if (lobby.Data.TryGetValue("RelayJoinCode", out DataObject relayJoinCode))
				{
					if (relayJoinCode != null)
					{
						relayManager.NewJoinCodeSet(relayJoinCode.Value);
						relayManager.StartClientWithJoinCode();
					}
				}
			}
			
			catch (LobbyServiceException e)
			{
				Debug.LogError($"Failed to join lobby: {e}");
			}
		}

		public async void JoinLobbyForBrowser_ButtonWrapper(string lobbyCode)
		{
			JoinLobbyForBrowser(lobbyCode);
		}
		
		public async Task JoinLobbyForBrowser(string lobbyCode)
		{
			Debug.Log("LobbyManager: Join code is: " +  lobbyCode);
			
			Debug.Log("Joining lobby...but not starting network yet");
			await authenticationManager.SignInAsync();
			
			lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
			
			//Set joining player's username
			await SetPlayerUsername(lobby);
			
			//Refresh lobby from server to stop client-side issues with names
			lobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);
			
			//Reset lobby data with new player info
			LobbyData lobbyData = UpdateLobbyData(lobby);
			
			//Set up callbacks
			await SetupLobbyEvents();
			
			//Call events to update UI
			LobbyEvents.OnLobbyUpdated?.Invoke(lobbyData);
			Debug.Log("LobbyManager: OnLobbyChanged has requested the player list to be refreshed.");
			
			try
			{
				//Get relay join code  but don't start networked client yet
				if (lobby.Data.TryGetValue("RelayJoinCode", out DataObject relayJoinCode))
				{
					if (relayJoinCode != null)
					{
						relayManager.NewJoinCodeSet(relayJoinCode.Value);
					}
				}
			}
			
			catch (LobbyServiceException e)
			{
				Debug.LogError($"Failed to join lobby: {e}");
			}
			
			//Call event to update screen
			LobbyEvents.WaitingForOtherPlayersToJoinLobby?.Invoke(lobby.Players.Count);
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
				Debug.Log("Lobby count = " + response.Results.Count);
				
				lobby = await LobbyService.Instance.JoinLobbyByIdAsync(response.Results[0].Id);

				lobby.Data.TryGetValue("RelayJoinCode", out DataObject relayJoinCode);
				if (relayJoinCode != null)
				{
					relayManager.NewJoinCodeSet(relayJoinCode.Value);
					relayManager.StartClientWithJoinCode();
				}
			}
			else
			{
				Debug.Log("No available lobbies found.");
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
			
			//update the lobby data class for UI to use
			obj.ApplyToLobby(lobby);
			LobbyData lobbyData = UpdateLobbyData(lobby);
			
			//Call event to update UI
			LobbyEvents.OnLobbyUpdated?.Invoke(lobbyData);
			Debug.Log("LobbyManager: OnLobbyChanged has requested the player list to be refreshed.");
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

		public async Task SetPlayerUsername(Lobby lobby)
		{
			inputUsername = playerNameScript.Username;
			
			try
			{
				UpdatePlayerOptions playerOptions = new UpdatePlayerOptions();

				playerOptions.Data = new Dictionary<string, PlayerDataObject>
				{
					{
						"Username",
						new PlayerDataObject(
							visibility: PlayerDataObject.VisibilityOptions.Public,
							value: inputUsername)
					}
				};
				
				//Ensure you sign-in before calling Authentication Instance
				//See IAuthenticationService interface
				string playerId = AuthenticationService.Instance.PlayerId;

				await LobbyService.Instance.UpdatePlayerAsync(lobby.Id, playerId, playerOptions);
				
			}
			catch (LobbyServiceException e)
			{
				Debug.Log("Failed to set player username: " + e);
			}
			
		}

		public async Task SetLobbyJoinCode(Lobby lobby)
		{
			try
			{
				//Lobby code is null immediately after creation - need to try a few times to get it to work
				int attempts = 0;
				while (string.IsNullOrEmpty(lobby.LobbyCode) && attempts < 10)
				{
					Debug.Log("LobbyManager: Waiting for lobby code");
					await Task.Delay(200);
					attempts++;
				}

				if (string.IsNullOrEmpty(lobby.LobbyCode))
				{
					Debug.LogError("LobbyManager: Lobby code is still null after waiting");
					return;
				}
				
				UpdateLobbyOptions updateOptions =  new UpdateLobbyOptions();
				updateOptions.Data = new Dictionary<string, DataObject>
				{
					{
						"LobbyJoinCode",
						new DataObject(visibility: DataObject.VisibilityOptions.Public, value: lobby.LobbyCode)
					}
				};

				await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, updateOptions);
				Debug.Log("LobbyManager: Lobby join code is " + lobby.LobbyCode);
			}
			catch (Exception e)
			{
				Debug.LogError("Failed to set Lobby Join Code" + e);
				throw;
			}
			
		}

		public async Task SetLobbyRelayCode(Lobby lobby)
		{
			string relayJoinCode = relayManager.reservedRelayJoinCode;
			
			try
			{
				UpdateLobbyOptions updateOptions = new UpdateLobbyOptions();
				updateOptions.Data = new Dictionary<string, DataObject>
				{
					{
						"RelayJoinCode",
						new DataObject(visibility: DataObject.VisibilityOptions.Public, value: relayJoinCode)
					}
				};
				
				await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, updateOptions);
				Debug.Log("LobbyManager: Relay join code is should be set as "  + relayJoinCode);
			}
			catch (Exception e)
			{
				Debug.LogError("Failed to set Lobby Join Code" + e);
				throw;
			}
		}

		public async Task InitialLobbyUpdate(Lobby lobby)
		{
			//Save Player Name
			await SetPlayerUsername(lobby);
			
			//Save allocated relay join code
			await SetLobbyRelayCode(lobby);
			
			//Save lobby join code
			await SetLobbyJoinCode(lobby);
			
			UpdateLobbyData(lobby);

		}

		public LobbyData UpdateLobbyData(Lobby lobby)
		{
			LobbyData lobbyData = new LobbyData();
			lobbyData.LobbyName =  lobby.Name;
			lobbyData.PlayerCount = lobby.Players.Count;
			lobbyData.RelayJoinCode = lobby.Data?["RelayJoinCode"]?.Value;
			lobbyData.LobbyJoinCode = lobby.LobbyCode;
			
			//Collect player usernames
			lobbyData.PlayerNames = new List<string>();

			foreach (var player in lobby.Players)
			{
				if (player.Data != null && player.Data.TryGetValue("Username", out PlayerDataObject username))
				{
					lobbyData.PlayerNames.Add(username.Value);
				}
			}
			
			//Check if player is host
			string localPlayerID = AuthenticationService.Instance.PlayerId;
			lobbyData.isHost = lobby.HostId == localPlayerID;
			
			//Call event to update UI
			LobbyEvents.OnLobbyUpdated?.Invoke(lobbyData);
			
			return lobbyData; 
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
		
		//Adding separate function for Query Lobbies, may be integrated or possibly replace Query Lobbies original
		//I didn't understand some of the Key-Value pairings in the suggested lobby function so have used a list and data class instead
		public async Task QueryLobbiesForLobbyBrowser(List<LobbyData> lobbyInfoList )
		{
			//Check initialization
			if (UnityServices.State != ServicesInitializationState.Initialized)
			{
				Debug.Log("LobbyManager: Unity Services not initialized. Initializing now.");
				await UnityServices.InitializeAsync();
			}

			if (!AuthenticationService.Instance.IsSignedIn)
			{
				Debug.Log("LobbyManager: Signed In Failed");
				await authenticationManager.SignInAsync();
			}
			
			//Clear old list entries
			lobbyInfoList.Clear();
			
			QueryLobbiesOptions options = new QueryLobbiesOptions();
			{
				options.Count = 25; //max lobbies to return
				
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
				Debug.Log("LobbyManager: Lobby Query found " + lobbies.Results.Count + " lobbies");

				foreach (Lobby foundLobby in lobbies.Results)
				{
					//Create a new data container object for the lobby
					LobbyData lobbyData = new LobbyData();
					
					//Set details of each field in LobbyData
					lobbyData.LobbyName = foundLobby.Name; //Name
					
					if (foundLobby.Players.Count > 0) //Player Count
					{
						lobbyData.PlayerCount = foundLobby.Players.Count;
					}
					else
					{
						lobbyData.PlayerCount = 0; //Shouldn't happen as sorting for open lobbies only
						Debug.LogWarning("LobbyManager: Lobby query returned 0 players in an active lobby.");
					}

					//Relay Join Code
					lobbyData.RelayJoinCode = ""; //Set to string so it doesn't null for key / dictionary
					if (foundLobby.Data != null && foundLobby.Data.TryGetValue("RelayJoinCode", out DataObject relayJoinCodeDataObject))
					{
						lobbyData.RelayJoinCode = relayJoinCodeDataObject.Value;
						Debug.Log("LobbyManager: Lobby Relay Code for found lobby is: " + lobbyData.RelayJoinCode);
					}
					
					//Lobby Join Code
					lobbyData.LobbyJoinCode = "";//Set to string so it doesn't null for key / dictionary
					if (foundLobby.Data != null &&
					    foundLobby.Data.TryGetValue("LobbyJoinCode", out DataObject joinCodeDataObject))
					{
						lobbyData.LobbyJoinCode = joinCodeDataObject.Value;
						Debug.Log("LobbyManager: Lobby Join Code for found lobby is " +  lobbyData.LobbyJoinCode);
					}
					
					//Player Info
					lobbyData.PlayerNames = new List<string>();
					foreach (Player player in foundLobby.Players)
					{
						if (player.Data != null &&
						    player.Data.TryGetValue("Username", out PlayerDataObject playerDataObject))
						{
							lobbyData.PlayerNames.Add(playerDataObject.Value);
							Debug.Log("LobbyManager: Player in " + lobbyData.LobbyName + "is named: " +  playerDataObject.Value);
						}
						else
						{
							lobbyData.PlayerNames.Add("Unnamed Player");
						}
					}
					
					//Add the lobbyData container to the list
					lobbyInfoList.Add(lobbyData);
				}
			}
		}
		
		private async Task SetupLobbyEvents()
		{
			Debug.Log("Setting up Lobby events");
			var callbacks = new LobbyEventCallbacks();
			callbacks.LobbyChanged += OnLobbyChanged;
			
			//callbacks.DataChanged += CallbacksOnDataChanged;
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
			Debug.Log("Data changed event : " + obj);
			
			// TODO more than just relaycode, presumably lobby name changes etc
			LobbyData lobbyData = UpdateLobbyData(lobby);
			
			//Lobby name
			if (obj.TryGetValue("LobbyName", out ChangedOrRemovedLobbyValue<DataObject> lobbyNameDataObject))
			{
				
			}
			
			//Relay Code
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