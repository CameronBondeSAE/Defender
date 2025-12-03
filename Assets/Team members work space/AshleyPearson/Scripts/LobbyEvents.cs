using System;
using UnityEngine;

namespace AshleyPearson
{
    //Lobby-related events to assist UI and passing information
    
    public class LobbyEvents : MonoBehaviour
    {
        public static Action<string> OnButtonClicked_JoinLobby;
        public static Action OnButtonClicked_RefreshLobbyList;
        public static Action<int> WaitingForOtherPlayersToJoinLobby;
    }
}
