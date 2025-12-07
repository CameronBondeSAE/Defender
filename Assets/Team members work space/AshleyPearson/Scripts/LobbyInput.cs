using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AshleyPearson
{

    public class LobbyInput: MonoBehaviour
    {
        public TMP_InputField lobbyNameInputField;
        [SerializeField] private string lobbyName;
        [SerializeField] private Button createLobbyButton;

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
            
            //Turn off button so they can't keep clicking it
            createLobbyButton.interactable = false;

        }

        public string LobbyName
        {
            get { return lobbyName; }
        }
    }
}
