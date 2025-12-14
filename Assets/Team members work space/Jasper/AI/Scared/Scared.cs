using System;
using System.Collections;
using UnityEngine;

namespace Jasper_AI
{
    public class Scared : MonoBehaviour
    {
        private Look _look;
        private AboveHeadDisplay _aboveHeadDisplay;
        [SerializeField] private float scaredRadius;
        private GPTConnection _gptConn;
        [SerializeField] private LayerMask scaredOf;

        private string gptMessage = "Oh no scary";
        private bool gptCanUpdate = true; 

        void OnEnable()
        {
            _look = GetComponent<Look>();
            _aboveHeadDisplay = GetComponentInChildren<AboveHeadDisplay>();
            _gptConn = GetComponent<GPTConnection>();
            _gptConn.OnGotResponse += OnGotResponse;
            _aboveHeadDisplay.ChangeMessage("");
        }

        private void OnDisable()
        {
            _gptConn.OnGotResponse -= OnGotResponse;
        }

        // Update is called once per frame
        void Update()
        {
            //if there is anything around display the message 
            if (_look.LookAround(scaredOf).Count > 0)
            {
                _aboveHeadDisplay.ChangeMessage(gptMessage);
                if (gptCanUpdate) //if the cooldown is over get a new message 
                {
                    StartCoroutine(GPTUpdate());
                }
            }
            else
            {
                _aboveHeadDisplay.ChangeMessage("");
            }
        }

        private void OnGotResponse(string response)
        {
            gptMessage = response;
        }

        IEnumerator GPTUpdate()
        {
            //_gptConn.GetResponse("tell me a three word sentence someone scared would say");
            gptCanUpdate = false;
            yield return new WaitForSeconds(60 * 5); //wait a few minutes between calls
            gptCanUpdate = true;
        }
    }
}
