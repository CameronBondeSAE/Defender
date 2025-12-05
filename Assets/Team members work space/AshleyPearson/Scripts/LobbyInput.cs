using TMPro;
using UnityEngine;

namespace AshleyPearson
{

    public class LobbyInput: MonoBehaviour
    {
        public TMP_InputField lobbyNameInputField;
        [SerializeField] string lobbyName;

        public void OnSubmitLobbyName()
        {
            Debug.Log("LobbyInput: Lobby Name Submit Button has been clicked");
            lobbyName = lobbyNameInputField.text;

            if (string.IsNullOrEmpty(lobbyName))
            {
                Debug.LogWarning("LobbyInput: Please enter a lobby name.");
                return;
            }
            
            Debug.Log("LobbyInput: Lobby Name is "  + lobbyName);

            LobbyEvents.OnButtonClicked_HostGame?.Invoke(lobbyName);
            Debug.Log("LobbyInput: Host has requested to create lobby");

        }

        public string LobbyName
        {
            get { return lobbyName; }
        }
    }
}
