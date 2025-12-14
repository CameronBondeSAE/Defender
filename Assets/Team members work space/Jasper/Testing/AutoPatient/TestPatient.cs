using System;
using UnityEngine;

public class TestPatient : MonoBehaviour
{
    [SerializeField] private Material healedMat;
    [SerializeField] private Material unhealedMat;

    private Health _health;
    private Renderer _renderer;
    
    private void OnEnable()
    {
        _health = gameObject.GetComponent<Health>();
        _renderer = GetComponent<Renderer>();
        _health.currentHealth.OnValueChanged += HealthChanged;
    }

    private void Start()
    {
        _health.TakeDamage(7);
    }

    private void OnDisable()
    {
        _health.currentHealth.OnValueChanged -= HealthChanged;
    }

    private void HealthChanged(float previousHealth, float newHealth)
    {
        if (newHealth >= _health.maxHealth)
        {
            _renderer.material = healedMat; 
        }
        else
        {
            _renderer.material = unhealedMat;
        }
    }
}
