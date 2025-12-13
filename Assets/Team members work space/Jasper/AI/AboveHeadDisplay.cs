using System;
using TMPro;
using UnityEngine;

public class AboveHeadDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;

    private void Start()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    public void ChangeMessage(string message)
    {
        messageText.text = message; 
    }
}
