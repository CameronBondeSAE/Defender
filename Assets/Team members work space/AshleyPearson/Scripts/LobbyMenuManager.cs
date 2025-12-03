using UnityEngine;
using UnityEngine.UI;

namespace AshleyPearson
{
    //Used to switch between menu groups
    public class LobbyMenuManager : MonoBehaviour
    {
       //Buttons
       [SerializeField] private GameObject usernameMenuGroup;
       
       [SerializeField] private GameObject hostMenuGroup;
       [SerializeField] private GameObject waitingForPlayersGroup;
       [SerializeField] private GameObject waitingForPlayersBackButton;
       [SerializeField] private Text playersJoinedText;
       
       [SerializeField] private GameObject joinGameMenuGroup;

       private void OnEnable()
       {
           LobbyEvents.WaitingForOtherPlayersToJoinLobby += WaitingForOtherPlayers_UI;
       }

       private void OnDisable()
       {
           LobbyEvents.WaitingForOtherPlayersToJoinLobby -= WaitingForOtherPlayers_UI;
       }

       private void WaitingForOtherPlayers_UI(int playerCount)
       {
           Debug.Log("LobbyMenuManager: Host is waiting for other players to join");
           
           //Turn on/off other menus
           hostMenuGroup.SetActive(false);
           waitingForPlayersGroup.SetActive(true);
           
           //Turn off button so host can't leave until others join OR fix later so that going back cancels the lobby
           waitingForPlayersBackButton.SetActive(false); 
           
           //Set the text for the player number
           playersJoinedText.text = playerCount + " /4 Players Joined";
           
           
       }
    }
}
