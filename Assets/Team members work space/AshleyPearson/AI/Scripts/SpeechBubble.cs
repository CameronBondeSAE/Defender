using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace AshleyPearson
{
    public class SpeechBubble : NetworkBehaviour
    {
        [SerializeField] TMP_Text reportText;
        [SerializeField] RawImage speechBubble;
        
        private bool isBubbleCoroutineRunning = false;
        private float waitTime = 2f;
        
        
        private void Awake()
        {
            //Turn everything off to start 
            reportText.gameObject.SetActive(false);
            speechBubble.gameObject.SetActive(false);
        }
        
        public override void OnNetworkSpawn()
        {
            ScoutEvents.OnReport += HandleReport_Server;
            Debug.Log("[SpeechBubble] Server has subscribed to OnReport");
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                ScoutEvents.OnReport -= HandleReport_Server;
                Debug.Log("[SpeechBubble] Server has UNsubscribed to OnReport");
            }
        }

        private void OnDisable()
        {
            if (IsServer)
            {
                ScoutEvents.OnReport -= HandleReport_Server;
            }
        }

        private void HandleReport_Server(int alienCount)
        {
            //Receives the report and forwards result to all clients
            if (IsServer)
            {
                Debug.Log("[SpeechBubble] Server is implementing the Scout's report");

                //Display the speech bubble
                DisplayReport_RPC(alienCount);
            
                //Wait for a bit, then remove
                if (!isBubbleCoroutineRunning)
                {
                    StartCoroutine(BubbleCoroutine());
                }
            }

            else
            {
                
            }
        }
        
        [Rpc(SendTo.ClientsAndHost, Delivery =  RpcDelivery.Reliable, RequireOwnership = true)]
        private void DisplayReport_RPC(int alienCount)
        {
            if (reportText == null || speechBubble == null)
            {
                Debug.LogWarning("[SpeechBubble] Either report text or speech bubble is null.");
                return;
            }
            
            //Update text
            reportText.text = (alienCount + " Aliens Incoming!");
            Debug.Log("[SpeechBubble] Speech bubble should be updated now.");

            //Set active
            speechBubble.gameObject.SetActive(true);
            reportText.gameObject.SetActive(true);
        }
        
        [Rpc(SendTo.ClientsAndHost, Delivery =  RpcDelivery.Reliable, RequireOwnership = false)]
        private void HideReport_RPC()
        {
            //Deactivate stuff
            Debug.Log("[SpeechBubble] Speech bubble should be hidden now.");
            speechBubble.gameObject.SetActive(false);
            reportText.gameObject.SetActive(false);
        }

        private IEnumerator BubbleCoroutine()
        {
            isBubbleCoroutineRunning = true;

            yield return new WaitForSeconds(waitTime);
            
            HideReport_RPC();
            isBubbleCoroutineRunning = false;
        }
    }
}
