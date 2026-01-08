using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI currentText;
    
    [Header("Player Finance Reference")]
    [SerializeField] private PlayerFinance playerFinance;
    
    [Header("Display Settings")]
    [Tooltip("Format string for displaying payday. {0} will be replaced with the payday value.")]
    [SerializeField] private string paydayFormat = "RM{0:F0}";
    
    private void Start()
    {
        // Find CurrentText if not assigned
        if (currentText == null)
        {
            // Try to find it in children
            currentText = GetComponentInChildren<TextMeshProUGUI>();
            
            // If still null, try to find by name
            if (currentText == null)
            {
                Transform currentTextTransform = transform.Find("LowerBanner/CurrentText");
                if (currentTextTransform != null)
                {
                    currentText = currentTextTransform.GetComponent<TextMeshProUGUI>();
                }
            }
            
            // Last resort: search by name in scene
            if (currentText == null)
            {
                GameObject currentTextObj = GameObject.Find("CurrentText");
                if (currentTextObj != null)
                {
                    currentText = currentTextObj.GetComponent<TextMeshProUGUI>();
                }
            }
        }
        
        // Find PlayerFinance if not assigned
        if (playerFinance == null)
        {
            // Try to find on the same GameObject
            playerFinance = GetComponent<PlayerFinance>();
            
            // If not found, try to find on Player GameObject
            if (playerFinance == null)
            {
                GameObject playerObj = GameObject.Find("Player");
                if (playerObj != null)
                {
                    playerFinance = playerObj.GetComponent<PlayerFinance>();
                }
            }
            
            // Last resort: find any PlayerFinance in scene
            if (playerFinance == null)
            {
                playerFinance = FindAnyObjectByType<PlayerFinance>();
            }
        }
        
        // Subscribe to payday changes
        if (playerFinance != null)
        {
            playerFinance.OnPaydayChanged += UpdateCurrentText;
            // Update immediately with current value
            UpdateCurrentText(playerFinance.CurrentPayday);
        }
        else
        {
            Debug.LogWarning("PlayerUIController: PlayerFinance not found! CurrentText will not be updated.");
        }
    }
    
    private void UpdateCurrentText(float payday)
    {
        if (currentText != null)
        {
            currentText.text = string.Format(paydayFormat, payday);
        }
        else
        {
            Debug.LogWarning("PlayerUIController: CurrentText is null! Cannot update display.");
        }
    }
    
    /// <summary>
    /// Manually refreshes the CurrentText display with the current payday value
    /// </summary>
    public void RefreshDisplay()
    {
        if (playerFinance != null)
        {
            UpdateCurrentText(playerFinance.CurrentPayday);
        }
        else
        {
            Debug.LogWarning("PlayerUIController: PlayerFinance is null! Cannot refresh display.");
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from event to prevent memory leaks
        if (playerFinance != null)
        {
            playerFinance.OnPaydayChanged -= UpdateCurrentText;
        }
    }
}
