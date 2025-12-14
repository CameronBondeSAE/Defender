using System;
using TMPro;
using UnityEngine;

public class AboveHeadDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI healthText;

    private Health _health; 

    private void OnEnable()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;

        messageText.text = "";
        _health = GetComponentInParent<Health>();
        if (_health is not null)
        {
            healthText.text = _health.currentHealth.Value.ToString();
            _health.OnHealthChanged += ChangeHealth;
        }
    }

    private void OnDisable()
    {
        if (_health is null) return;
        _health.OnHealthChanged -= ChangeHealth;
    }

    public void ChangeMessage(string message)
    {
        messageText.text = message; 
    }

    private void ChangeHealth(float health)
    {
        if (healthText is null) return; 
        healthText.text = _health.currentHealth.Value.ToString();
    }
}
