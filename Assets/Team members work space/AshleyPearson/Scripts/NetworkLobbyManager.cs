using System;
using System.Threading.Tasks;
using CameronBonde;
using Unity.Netcode;
using UnityEngine;

namespace AshleyPearson
{
    public class NetworkLobbyManager : NetworkBehaviour
    {
        public RelayManager relayManager;
        public LobbyManager lobbyManager;

        private void Start()
        {
            Debug.Log("NetworkLobbyManager: IsHost: "  + IsHost);
        }

        //Called on button click
        public void HostStartGame()
        {
            // if (!NetworkManager.Singleton.IsHost)
            // {
            //     Debug.LogWarning("NetworkLobbyManager: Start game called but this client is not the host");
            //     return;
            // }

            Debug.Log("NetworkLobbyManager: Host starting game, starting relay server...");
            // StartRelayHost();
        }

        public async Task StartRelayHost()
        {
            string joinCode = await relayManager.StartHostWithReservedRelay("udp");

            //Pass join code and tell clients to start on their side
            Debug.Log("NetworkLobbyManager: Using reserved join code"  + joinCode);
            ReceiveJoinCodeClient_ClientRpc(joinCode);
        }

        [ClientRpc(Delivery = RpcDelivery.Reliable)]
        private void ReceiveJoinCodeClient_ClientRpc(string joinCode)
        {
            if (!IsHost)
            {
                //Pass join code and tell clients to start on their side
                Debug.Log("ClientStarter: Received relay join code from host: " + joinCode);
                relayManager.NewJoinCodeSet(joinCode);
                relayManager.StartClientWithJoinCode();
            }
        }
    }
}
