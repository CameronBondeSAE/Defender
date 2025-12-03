using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CameronBonde;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

namespace AshleyPearson
{
    //Manages the refreshing of the list and moving lobby info from the lobby manager to the UI
    
    public class LobbyBrowserUI : MonoBehaviour
    {
       [SerializeField] private LobbyManager lobbyManager;
       [SerializeField] private LobbyEntry lobbyEntry;
       [SerializeField] private Transform lobbyContentParent;
       [SerializeField] private Button refreshButton;

       //Subscribe to any UI events that may be fired by buttons or other scripts
       private void OnEnable()
       {
           LobbyEvents.OnButtonClicked_RefreshLobbyList += RefreshLobbyList_ButtonWrapper;
       }

       private void OnDisable()
       {
           LobbyEvents.OnButtonClicked_RefreshLobbyList -= RefreshLobbyList_ButtonWrapper;
       }
       
       //Button wrapper is required due to the async Task nature of the main function
       private async void RefreshLobbyList_ButtonWrapper()
       {
           RefreshLobbyList();
       }

       //Actual refresh function
       private async Task RefreshLobbyList()
       {
           //Clear old entries
           foreach (Transform child in lobbyContentParent)
           {
               Destroy(child.gameObject);
               Debug.Log("LobbyBrowserUI: Old lobby entries destroyed");
           }

           //Query lobbies - pass in the empty list for the query function to fill
           List<LobbyData> lobbyList = new List<LobbyData>();
           await lobbyManager.QueryLobbiesForLobbyBrowser(lobbyList);

           //Check list is populated
           if (lobbyList.Count > 0) { Debug.Log("LobbyBrowserUI: Lobby list populated"); }
           else if (lobbyList == null || lobbyList.Count == 0) { Debug.Log("LobbyBrowserUI: Lobby list empty"); }

           //Spawn lobby prefabs under content parent
           foreach (LobbyData lobby in lobbyList)
           {
               LobbyEntry lobbyInstance = Instantiate(lobbyEntry, lobbyContentParent);
               Debug.Log("LobbyBrowserUI: Lobby prefab created");
               
               lobbyInstance.SetLobbyEntryInfo(lobby.LobbyName, lobby.PlayerCount, lobby.RelayJoinCode);
               Debug.Log("LobbyBrowserUI: Lobby prefab should be populated with information");
               
           }
       }
    }
}
