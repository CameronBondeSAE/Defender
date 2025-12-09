using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FireWallCoolDownUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image cooldownFillImage;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private GameObject cooldownPanel;
    
    [Header("Colors")]
    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color cooldownColor = Color.red;
    [SerializeField] private Color activeColor = Color.yellow;
    
    private ActiveFireWall fireWall;
    
    void Start()
    {
        // Hide UI initially
        if (cooldownPanel != null)
        {
            cooldownPanel.SetActive(false);
        }
    }
    
    public void Initialize(ActiveFireWall fireWallReference)
    {
        fireWall = fireWallReference;
    }
    
    public void UpdateCooldown(float remainingTime, float totalTime, bool isActive, bool isOnCooldown)
    {
        if (cooldownPanel != null)
        {
            // Show panel when active or on cooldown
            cooldownPanel.SetActive(isActive || isOnCooldown);
        }
        
        if (cooldownFillImage != null)
        {
            // Update fill amount (1 = full, 0 = empty)
            cooldownFillImage.fillAmount = remainingTime / totalTime;
            
            // Update color based on state
            if (isActive)
            {
                cooldownFillImage.color = activeColor;
            }
            else if (isOnCooldown)
            {
                cooldownFillImage.color = cooldownColor;
            }
            else
            {
                cooldownFillImage.color = readyColor;
            }
        }
        
        if (cooldownText != null)
        {
            if (isActive)
            {
                cooldownText.text = $"ACTIVE: {remainingTime:F1}s";
            }
            else if (isOnCooldown)
            {
                cooldownText.text = $"Cooldown: {remainingTime:F1}s";
            }
            else
            {
                cooldownText.text = "READY";
            }
        }
    }
    
    public void ShowReady()
    {
        if (cooldownPanel != null)
        {
            cooldownPanel.SetActive(false);
        }
    }
}
