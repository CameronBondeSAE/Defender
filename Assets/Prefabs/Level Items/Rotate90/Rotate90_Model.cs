using Defender;
using DG.Tweening;
using UnityEngine;
using Unity.Netcode;

public class Rotate90_Model : NetworkBehaviour, IUsable
{
    [Header("References")]
    public Health health;
    public RotateView view;
    [SerializeField] private Renderer wallRenderer;

    [Header("Color Settings")]
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color yellowColor = Color.yellow;
    [SerializeField] private Color orangeColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color redColor = Color.red;

    private bool _isDestroyed = false;
    
    private void Awake()
    {
        if (wallRenderer == null)
        {
            wallRenderer = GetComponentInChildren<Renderer>();
        }

        if (health == null)
        {
            health = GetComponent<Health>();
        }
        if (view == null)
        {
            view = GetComponent<RotateView>();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (health != null)
        {
            // Subscribe to NetworkVariable changes (called on ALL clients + server)
            health.currentHealth.OnValueChanged += OnHealthValueChanged;

            // Set initial color from current health
            UpdateColorFromHealth(health.currentHealth.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (health != null)
        {
            health.currentHealth.OnValueChanged -= OnHealthValueChanged;
        }
    }

    private void OnHealthValueChanged(float previous, float current)
    {
        UpdateColorFromHealth(current);
    }

    private void UpdateColorFromHealth(float healthValue)
    {
        if (health == null || wallRenderer == null) return;

        float ratio = (health.maxHealth > 0f)
            ? healthValue / health.maxHealth
            : 0f;

        if (ratio <= 0f)
        {
            // Wall is dead – only server should despawn
            if (IsServer)
            {
                DespawnWall();
            }
            return;
        }

        // Decide color based on thresholds
        if (ratio <= 0.25f)
        {
            wallRenderer.material.color = redColor;
        }
        else if (ratio <= 0.50f)
        {
            wallRenderer.material.color = orangeColor;
        }
        else if (ratio <= 0.75f)
        {
            wallRenderer.material.color = yellowColor;
        }
        else
        {
            wallRenderer.material.color = healthyColor;
        }
    }

    private void DespawnWall()
    {
        if (!IsServer) return;

        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true); // despawn & destroy
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Use(CharacterBase characterTryingToUse)
    {
        if (_isDestroyed || health.currentHealth.Value <= 0f)
        {
            Debug.Log(name + " is destroyed and cannot be used.");
            return;
        }

        Debug.Log(name + " : " + characterTryingToUse.name + " is using");

        // Rotate 90 degrees over 2 seconds
        float newY = transform.eulerAngles.y + 90f;
        transform.DORotate(new Vector3(0f, newY, 0f), 2f, RotateMode.Fast);

        // Existing networked effects
        if (view != null)
        {
            view.stoneSound_RPC();
            view.stoneEmitRight_RPC();
            view.stoneEmitLeft_RPC();
        }
    }

    public void StopUsing()
    {
        Debug.Log(name + " stopped using");
    }
}
