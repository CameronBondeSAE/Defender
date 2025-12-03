using System;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

namespace AshleyPearson
{
    //Script on the lobby entry prefab that populates data and changes UI from the list of lobby info - called externally
    
    public class LobbyEntry : MonoBehaviour
    {
        [SerializeField] private Text lobbyNameText;
        [SerializeField] private Text playerCountText;
        [SerializeField] private Button joinLobbyButton;

        private string joinCode;

        public void SetLobbyEntryInfo(string lobbyName, int playerCount, string joinCode)
        {
            //Set lobby entry information for UI from network manager / relay manager
            lobbyNameText.text = lobbyName;
            playerCountText.text = playerCount.ToString();
            this.joinCode = joinCode;
            
            Debug.Log("LobbyEntry: Lobby Name " +  lobbyName +  " - JoinCode is " + joinCode);
        }

        public void OnButtonClicked_JoinLobby()
        {
            //Use static event to join relevant session
            LobbyEvents.OnButtonClicked_JoinLobby?.Invoke(this.joinCode);
        }

    }
}
