using System.Reflection;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using Unity.Netcode;
using DanniLi;

namespace AshleyPearson
{
    //Used to switch between menu groups
    public class LobbyMenuManager : MonoBehaviour
    {
       [Header("Lobby Menu Groups")]
       [SerializeField] private GameObject usernameMenuGroup;
       [SerializeField] private GameObject hostMenuGroup;
       [SerializeField] private GameObject waitingForPlayersGroup;
       [SerializeField] private GameObject waitingForPlayersBackButton;
       [SerializeField] private Text playersJoinedText;
       [SerializeField] private GameObject joinGameMenuGroup;
       
       [Header("Level Loading")]
       [SerializeField] private LevelLoader levelLoader;
       [SerializeField] private NetworkLobbyManager networkLobbyManager;

       //Used to determine which UI to show after username entered
       private System.Action actionAfterUsernameEntered;
       public PlayerName playerNameScript;

       private void OnEnable()
       {
           LobbyEvents.WaitingForOtherPlayersToJoinLobby += WaitingForOtherPlayers_UI;
           LobbyEvents.OnUsernameEntered += PerformAction;
           LobbyEvents.OnButtonClicked_HostStartedGame += HideAllScreens;
       }

       private void OnDisable()
       {
           LobbyEvents.WaitingForOtherPlayersToJoinLobby -= WaitingForOtherPlayers_UI;
           LobbyEvents.OnUsernameEntered -= PerformAction;
           LobbyEvents.OnButtonClicked_HostStartedGame -= HideAllScreens;
       }

       private void Start()
       {
           HideAllScreens();
       }

       private void HideAllScreens()
       {
           usernameMenuGroup.SetActive(false);
           hostMenuGroup.SetActive(false);
           waitingForPlayersGroup.SetActive(false);
           waitingForPlayersBackButton.SetActive(false);
           joinGameMenuGroup.SetActive(false);
       }

       public void OnButtonClick_CreateLobby()
       {
           actionAfterUsernameEntered = HostMenuScreen;
           Debug.Log("LobbyMenuManager: Host Game clicked");

           if (playerNameScript.Username.IsNullOrEmpty())
           {
               Debug.Log("LobbyMenuManager: Username is null or empty. Prompting username entry.");
               ShowUsernameScreen();
           }

           else
           {
               PerformAction();
               Debug.Log("LobbyMenuManager: Action to be performed is " + actionAfterUsernameEntered.GetMethodInfo().Name);
           }
       }
       
       public void OnButtonClick_LobbyBrowser()
       {
           actionAfterUsernameEntered = JoinMenuScreen;
           Debug.Log("LobbyMenuManager: Join Game clicked");
           
           if (playerNameScript.Username.IsNullOrEmpty())
           {
               Debug.Log("LobbyMenuManager: Username is null or empty. Prompting username entry.");
               ShowUsernameScreen();
           }
           else
           {
               PerformAction();
               Debug.Log("LobbyMenuManager: Action to be performed is " + actionAfterUsernameEntered.GetMethodInfo().Name);
           }
          
       }

       public void OnButtonClick_HostStartedGame()
       {
	       // CAM HACK: Is there where the host should start?
	       networkLobbyManager.HostStartGame();
	       
	       
           // //This method doesn't check for host as only the host should have access to the button in the first place
           LobbyEvents.OnButtonClicked_HostStartedGame?.Invoke();

           // only host can start level loading
           if (levelLoader != null &&
               NetworkManager.Singleton != null &&
               NetworkManager.Singleton.IsHost)
           {
               levelLoader.LoadFirstLevelServerRpc();
           } else
           {
               Debug.LogWarning("HostStartGame clicked, but NetworkManager is not running as host yet. " +
                                "Make sure Relay/Netcode host is started before loading the level.");
           }
       }
       
       private void ShowUsernameScreen()
       {
           //Username screen is shown regardless of which button is clicked
           usernameMenuGroup.SetActive(true);
       }
       
       //Action to perform after username is entered is stored
       private void PerformAction()
       {
           actionAfterUsernameEntered?.Invoke();
       }
      

       private void HostMenuScreen()
       {
           //Ensure no other lobby menu screen is active
           usernameMenuGroup.SetActive(false);
           joinGameMenuGroup.SetActive(false);
           waitingForPlayersGroup.SetActive(false);
           
           //Open host menu
           hostMenuGroup.SetActive(true);
       }

       private void JoinMenuScreen()
       {
           //Ensure no other lobby menu screen is active
           usernameMenuGroup.SetActive(false);
           hostMenuGroup.SetActive(false);
           waitingForPlayersGroup.SetActive(false);
           
           //Open join menu
           joinGameMenuGroup.SetActive(true);
           
           //Setup lobby info
           LobbyEvents.OnMenuButtonClicked_RefreshLobbyList?.Invoke();
           Debug.Log("LobbyMenuManager: Refreshing lobby");
       }
       
  
       private void WaitingForOtherPlayers_UI(int playerCount)
       {
           Debug.Log("LobbyMenuManager: Host is waiting for other players to join");
           
           //Turn on/off other menus
           hostMenuGroup.SetActive(false);
           joinGameMenuGroup.SetActive(false);
           waitingForPlayersGroup.SetActive(true);
           
           //Turn off button so host can't leave until others join OR fix later so that going back cancels the lobby
           waitingForPlayersBackButton.SetActive(false); 
           
       }
    }
}
