using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace AshleyPearson
{
    //Data class used to store lobby data pulled from server, then passed to UI
    
    public class LobbyData
    {
        public string LobbyName;
        public int PlayerCount;
        public string RelayJoinCode;
        public string LobbyJoinCode;
        public List<string> PlayerNames; //Not currently used, may be used on Host Lobby UI to show who has joined
    }
}
