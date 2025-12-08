using System;
using UnityEngine;

namespace AshleyPearson
{
    //Lobby-related events to assist UI and passing information
    
    public class LobbyEvents : MonoBehaviour
    {
        //Networked - Create / Join Buttons
        public static Action<string> OnButtonClicked_JoinGame;
        public static Action<string> OnButtonClicked_HostGame;
        
        //Host Menu Buttons
        public static Action<int> WaitingForOtherPlayersToJoinLobby;
        public static Action OnButtonClicked_HostStartedGame;
        
        //Join Menu Buttons
        public static Action OnMenuButtonClicked_RefreshLobbyList;
        
        //Username Menu Buttons
        public static Action OnUsernameEntered;
        
        //Lobby Update
        public static Action<LobbyData> OnLobbyUpdated;
    }
}
