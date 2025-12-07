using UnityEngine;
using CameronBonde;
using System.Collections.Generic;
using UnityEngine.UI;

namespace AshleyPearson
{
    public class WaitingRoomUI : MonoBehaviour
    {
        [SerializeField] GameObject playerEntryPrefab;
        [SerializeField] Transform playerListContentParent;
        [SerializeField] Text playerCountText;
        
        [SerializeField] List<GameObject> spawnedPlayerEntries = new List<GameObject>();

        private void OnEnable()
        {
            LobbyEvents.OnLobbyUpdated += RefreshFromLobbyData;
        }

        private void OnDisable()
        {
            LobbyEvents.OnLobbyUpdated -= RefreshFromLobbyData;
        }

        private void RefreshFromLobbyData(LobbyData lobbyData)
        {
            Debug.Log("Refreshing player list");

            //Clear old entries
            foreach (var playerEntry in spawnedPlayerEntries)
            {
                Destroy(playerEntry);
            }

            spawnedPlayerEntries.Clear();

            //Early out if there is no lobby active - which shouldn't be the case anyway
            if (lobbyData == null || lobbyData.PlayerNames == null)
            {
                Debug.Log("WaitingRoomUI: Lobby data is null or player list is empty");
                return;
            }
            
            Debug.Log("WaitingRoomUI: Lobby data contains " + lobbyData.PlayerNames.Count);
            
            //Change player count text
            playerCountText.text = (lobbyData.PlayerCount + "/4 Players Joined");
            
            foreach (var player in lobbyData.PlayerNames)
            {
                //Set default so not null
                string playerName = "PlayerTest";
                
                //Spawn UI prefab for each name entry
                GameObject playerEntry = Instantiate(playerEntryPrefab, playerListContentParent);
                
                //Update the names of players joined to the lobby
                Text playerNameTextField = playerEntry.GetComponentInChildren<Text>();
                Debug.Log("WaitingRoomUI: Text field is called " + playerNameTextField.gameObject.name);
                if (playerNameTextField != null)
                {
                    playerNameTextField.text = player;
                    Debug.Log("WaitingRoom: Player Name should be changed now");
                }

                else
                {
                    Debug.LogWarning("WaitingRoom: PlayerName text field does not exist");
                }
                
                spawnedPlayerEntries.Add(playerEntry);
            }
        }
        
    }
}
