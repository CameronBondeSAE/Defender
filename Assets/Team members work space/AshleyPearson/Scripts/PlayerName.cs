using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

namespace AshleyPearson
{
    public class PlayerName: MonoBehaviour
    {
        //Modifiable variable for the local player's name to go into
        [SerializeField] private string username;
        public TMP_InputField usernameInputField;
        private string enteredUsername;

        private void Start()
        {
            enteredUsername = null;
        }
        
        public void OnSubmitName()
        {
            Debug.Log("PlayerName: Username Button has been clicked");
            enteredUsername = usernameInputField.text;
            
            if (string.IsNullOrEmpty(enteredUsername))
            {
                Debug.LogWarning("PlayerName: Please enter a name.");
                return;
            }
            
            username = enteredUsername;
            Debug.Log("PlayerName: Player name was saved as " + username);
            
            LobbyEvents.OnUsernameEntered?.Invoke();
            Debug.Log("PlayerName: Username entered event was invoked");
        }
        
        //For public access to username
        public string Username
        {
            get { return username; }
        }
    }
}
