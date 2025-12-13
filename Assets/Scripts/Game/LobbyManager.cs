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
using Unity.Netcode;
using AshleyPearson;

namespace CameronBonde
{
	public class LobbyManager : MonoBehaviour
	{
		public string lobbyName      = "Defender Lobby #1";
		public string playerName = "CAM";
		public int    maxPlayers     = 4;
		public float  heartBeatDelay = 15f;
		
		private Lobby currentLobby;
		private float heartbeatInterval = 15f;
		private float heartbeatTimer;
		
		public RelayManager relayManager;
		public AuthenticationManager authenticationManager;
		
		public Lobby lobby;
		
		//Added name variables
		public string inputLobbyName;
		public string inputUsername;
		
		public PlayerName playerNameScript;
	
		#region 1) Start/wait for Services/Authentication and ui wiring
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
		#endregion

		#region 2) Host lobby creation loguic
		public async void CreateLobby(string inputLobbyName)
		{
			Debug.Log("Creating lobby...");
			
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

			CreateLobbyOptions options = new CreateLobbyOptions();
			options.IsPrivate = false;
			options.IsLocked  = false;
			options.Data      = new Dictionary<string, DataObject>();

			lobby = await LobbyService.Instance.CreateLobbyAsync(inputLobbyName, maxPlayers, options);

			// Set up lobby events so we get callbacks when data changes
			await SetupLobbyEvents();

			// Initial data (player name + lobby code, but NO relay join code yet)
			await InitialLobbyUpdate(lobby, false);

			// Heartbeat the lobby every 15 seconds.
			StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, heartBeatDelay));

			// Show waiting-for-players UI
			LobbyEvents.WaitingForOtherPlayersToJoinLobby?.Invoke(lobby.Players.Count);
		}
		#endregion

		#region 3) Client join lobby logic (immediate netcode join & browser join)
		// so client joins a lobby and either starts netcode immediately if RelayJoinCode exists, or waits for host to publish it
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
			await SetPlayerUsername(lobby);
			lobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);
			// LobbyData lobbyData = UpdateLobbyData(lobby);
			// await SetupLobbyEvents();
			// LobbyEvents.OnLobbyUpdated?.Invoke(lobbyData);
			// Debug.Log("LobbyManager: OnLobbyChanged has requested the player list to be refreshed.");
			//
			// try
			// {
			// 	//get relay join code  but DONT start networked client yet
			// 	if (lobby.Data.TryGetValue("RelayJoinCode", out DataObject relayJoinCode))
			// 	{
			// 		if (relayJoinCode != null)
			// 		{
			// 			relayManager.NewJoinCodeSet(relayJoinCode.Value);
			// 		}
			// 	}
			// }
			//
			// catch (LobbyServiceException e)
			// {
			// 	Debug.LogError($"Failed to join lobby: {e}");
			// }
			//
			// //Call event to update screen
			// LobbyEvents.WaitingForOtherPlayersToJoinLobby?.Invoke(lobby.Players.Count);
			// Reset lobby data with new player info
			LobbyData lobbyData = UpdateLobbyData(lobby);
			await SetupLobbyEvents();
			LobbyEvents.OnLobbyUpdated?.Invoke(lobbyData);
			Debug.Log("LobbyManager: OnLobbyChanged has requested the player list to be refreshed.");

			// NO Netcode here, clients dont start untill the host writes RelayJoinCode
			LobbyEvents.WaitingForOtherPlayersToJoinLobby?.Invoke(lobby.Players.Count);
		}
		#endregion

		#region 4) Transitioning from lobby to game (host starts relay; clients auto-join via lobby events)
		public async void HostStartGameFromLobby()
		{
			if (lobby == null)
			{
				Debug.LogWarning("LobbyManager: No current lobby; cannot start game.");
				return;
			}
			
			if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
			{
				Debug.LogWarning("LobbyManager: NetworkManager already listening, ignoring extra StartGame");
				return;
			}

			try
			{
				Debug.Log("LobbyManager: Host starting Relay host + Netcode from lobby...");

				// Make sure we are signed in (should already be true)
				await authenticationManager.SignInAsync();

				// 1. start Relay + Netcode host (joinCode is set on relayManager)
				await relayManager.StartHostWithRelay(maxPlayers, "udp");

				// 2. write the join code into lobby data so clients can see it
				await SetLobbyRelayCode(lobby);

				// 3. actually/securely load the first level for everyone (host + clients)
				LevelLoader levelLoader = FindObjectOfType<LevelLoader>();
				if (levelLoader != null)
				{
					levelLoader.LoadFirstLevelServerRpc();
				}
				else
				{
					Debug.LogError("LobbyManager: LevelLoader not found");
				}
			}
			catch (Exception e)
			{
			}
		}

		void OnLobbyChanged(ILobbyChanges obj)
		{
			Debug.Log("Lobby changed event");

			if (obj.Data.Value != null)
			{
				foreach (var changedOrRemovedLobbyValue in obj.Data.Value)
				{
					Debug.Log(changedOrRemovedLobbyValue);
				}
			}
			obj.ApplyToLobby(lobby);
			LobbyData lobbyData = UpdateLobbyData(lobby);

			// UI
			LobbyEvents.OnLobbyUpdated?.Invoke(lobbyData);
			Debug.Log("LobbyManager: OnLobbyChanged has requested the player list to be refreshed.");
			// If RelayJoinCode is set and Netcode is not running on this client, join relay
			if (!string.IsNullOrEmpty(lobbyData.RelayJoinCode))
			{
				string localPlayerID = AuthenticationService.Instance.PlayerId;
				bool isHost = lobby.HostId == localPlayerID;

				if (!isHost && NetworkManager.Singleton != null && !NetworkManager.Singleton.IsListening)
				{
					Debug.Log("LobbyManager: Relay join code detected. Starting client with join code...");
					relayManager.NewJoinCodeSet(lobbyData.RelayJoinCode);
					relayManager.StartClientWithJoinCode();
				}
			}
		}
		#endregion

		#region 6) Shutdown helpers (cleanup lobby on quit)
		private void OnApplicationQuit()
		{
			if (lobby != null) LobbyService.Instance.DeleteLobbyAsync(lobby.Id);
		}
		#endregion

		#region helper for lobby status and quick-joins
		public async void HostMarkGameStarted()
		{
			if (lobby == null)
			{
				return;
			}

			try
			{
				// actually storing a relay join code
				if (lobby.Data == null || !lobby.Data.TryGetValue("RelayJoinCode", out DataObject relayCode) ||
				    string.IsNullOrEmpty(relayCode.Value))
				{
					await relayManager.GetRelayCode();
					await SetLobbyRelayCode(lobby);
				}

				var updateOptions = new UpdateLobbyOptions
				{
					Data = new Dictionary<string, DataObject>
					{
						{
							"GameStarted",
							new DataObject(
								visibility: DataObject.VisibilityOptions.Public,
								value: "1")
						}
					}
				};

				await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, updateOptions);
			}
			catch (LobbyServiceException e)
			{
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
		#endregion

		#region helpers to write lobby data and some ui stuff 
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
			string relayJoinCode = relayManager.joinCode;
			
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

		public async Task InitialLobbyUpdate(Lobby lobby, bool setRelayJoinCode)
		{
			await SetPlayerUsername(lobby);
			if (setRelayJoinCode)
			{
				await SetLobbyRelayCode(lobby);
			}
			// save lobby join code
			await SetLobbyJoinCode(lobby);

			UpdateLobbyData(lobby);
		}

		public LobbyData UpdateLobbyData(Lobby lobby)
		{
			LobbyData lobbyData = new LobbyData();

			lobbyData.LobbyName   = lobby.Name;
			lobbyData.PlayerCount = lobby.Players.Count;

			// read RelayJoinCode (it may not exist yet)
			lobbyData.RelayJoinCode = null;
			if (lobby.Data != null &&
			    lobby.Data.TryGetValue("RelayJoinCode", out DataObject relayCodeObj) &&
			    relayCodeObj != null)
			{
				lobbyData.RelayJoinCode = relayCodeObj.Value;
			}

			lobbyData.LobbyJoinCode = lobby.LobbyCode;

			// Collect player usernames
			lobbyData.PlayerNames = new List<string>();
			foreach (var player in lobby.Players)
			{
				if (player.Data != null &&
				    player.Data.TryGetValue("Username", out PlayerDataObject username))
				{
					lobbyData.PlayerNames.Add(username.Value);
				}
			}

			// Check if local player is host
			string localPlayerID  = AuthenticationService.Instance.PlayerId;
			lobbyData.isHost      = lobby.HostId == localPlayerID;
			LobbyEvents.OnLobbyUpdated?.Invoke(lobbyData);
			return lobbyData;
		}
		#endregion

		#region helpers for lobby querying/discovery, events, heartbeat, and bulk updates
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
		#endregion
	}
}